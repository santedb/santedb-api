﻿/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you
 * may not use this file except in compliance with the License. You may
 * obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 *
 * User: fyfej
 * Date: 2021-8-5
 */

using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Security.Principal;

namespace SanteDB.Core.Security.Privacy
{
    /// <summary>
    /// Local policy enforcement point service
    /// </summary>
    [ServiceProvider("Default Policy Enforcement Service")]
    public class DataPolicyFilterService : IPrivacyEnforcementService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Data Policy Enforcement Filter Service";

        // Security tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataPolicyFilterService));

        // Filter configuration
        private DataPolicyFilterConfigurationSection m_configuration;

        // Actions
        private ConcurrentDictionary<Type, ResourceDataPolicyFilter> m_actions = new ConcurrentDictionary<Type, ResourceDataPolicyFilter>();

        // PDP Service
        private IPolicyDecisionService m_pdpService;

        // Adhoc cache
        private IAdhocCacheService m_adhocCache;

        // Password Hashing
        private IPasswordHashingService m_hasher;

        // Data caching service
        private IDataCachingService m_dataCachingService;

        // Subscription executor
        private ISubscriptionExecutor m_subscriptionExecutor;

        // Threadpool service
        private IThreadPoolService m_threadPool;

        /// <summary>
        /// Data policy filter service with DI
        /// </summary>
        public DataPolicyFilterService(IConfigurationManager configurationManager, IPasswordHashingService passwordService, IPolicyDecisionService pdpService, IThreadPoolService threadPoolService,
            IDataCachingService dataCachingService, ISubscriptionExecutor subscriptionExecutor = null, IAdhocCacheService adhocCache = null)
        {
            this.m_hasher = passwordService;
            this.m_adhocCache = adhocCache;
            this.m_pdpService = pdpService;
            this.m_subscriptionExecutor = subscriptionExecutor;
            this.m_dataCachingService = dataCachingService;
            this.m_threadPool = threadPoolService;

            // Configuration load
            this.m_configuration = configurationManager.GetSection<DataPolicyFilterConfigurationSection>();

            if (this.m_configuration == null)
            {
                this.m_tracer.TraceWarning("No data policy configuration exists. Setting all to HIDE");
                this.m_configuration = new DataPolicyFilterConfigurationSection() { DefaultAction = ResourceDataPolicyActionType.Hide, Resources = new List<ResourceDataPolicyFilter>() };
            }

            if (this.m_configuration.Resources != null)
                foreach (var t in this.m_configuration.Resources)
                {
                    if (typeof(Act).IsAssignableFrom(t.ResourceType.Type) || typeof(Entity).IsAssignableFrom(t.ResourceType.Type) || typeof(AssigningAuthority).IsAssignableFrom(t.ResourceType.Type))
                    {
                        this.m_tracer.TraceInfo("Binding privacy action {0} to {1}", t.Action, t.ResourceType.Type);
                        this.m_actions.TryAdd(t.ResourceType.Type, t);
                    }
                }
        }

        /// <summary>
        /// Handle post query event
        /// </summary>
        public virtual IEnumerable<TData> Apply<TData>(IEnumerable<TData> results, IPrincipal principal) where TData : IdentifiedData
        {
            if (principal != AuthenticationContext.SystemPrincipal) // System principal does not get filtered
                return results
                    .Select(
                        o => this.Apply(o, principal)
                    );
            return results;
        }

        /// <summary>
        /// Gets the domains that <paramref name="principal"/> should be filtered
        /// </summary>
        private IEnumerable<AssigningAuthority> GetFilterDomains(IPrincipal principal)
        {
            String key = null;
            if (principal is IClaimsPrincipal cp && cp.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBSessionIdClaim))
                key = this.m_hasher.ComputeHash($"$aa.filter.{cp.FindFirst(SanteDBClaimTypes.SanteDBSessionIdClaim).Value}");
            else
                key = this.m_hasher.ComputeHash($"$aa.filter.{principal.Identity.Name}");

            var domainsToFilter = this.m_adhocCache?.Get<AssigningAuthority[]>(key);
            if (domainsToFilter == null)
            {
                var aaDp = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AssigningAuthority>>();
                var protectedAuthorities = aaDp?.Query(o => o.PolicyKey != null, AuthenticationContext.SystemPrincipal).ToList();
                domainsToFilter = protectedAuthorities
                        .Where(aa => this.m_pdpService.GetPolicyOutcome(principal, aa.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid) != PolicyGrantType.Grant)
                        .ToArray();
                this.m_adhocCache?.Add(key, domainsToFilter, new TimeSpan(0, 0, 60));
            }
            return domainsToFilter;
        }

        /// <summary>
        /// Apply actions to hid identity
        /// </summary>
        private void ApplyIdentifierFilter(IdentifiedData result, IPrincipal accessor)
        {
            if (!this.m_actions.TryGetValue(typeof(AssigningAuthority), out var policy) && !this.m_actions.TryGetValue(result.GetType(), out policy))
                policy = new ResourceDataPolicyFilter() { Action = this.m_configuration.DefaultAction };
            var domainsToFilter = this.GetFilterDomains(accessor);

            switch (policy.Action)
            {
                case ResourceDataPolicyActionType.Hide:
                case ResourceDataPolicyActionType.Nullify:
                    {
                        var r = (result as Act)?.Identifiers.RemoveAll(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey));
                        r += (result as Entity)?.Identifiers.RemoveAll(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey));
                        if (r > 0)
                        {
                            //AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.Policy.Oid, PolicyGrantType.Deny)).ToList()), true);
                            if (result is ITaggable tag)
                            {
                                tag.AddTag("$pep.masked", "true");
                                tag.AddTag("$pep.method", "hide");
                            }
                        }
                        break;
                    }
                case ResourceDataPolicyActionType.Hash:
                case ResourceDataPolicyActionType.Hash | ResourceDataPolicyActionType.Audit:
                    {
                        var r = 0;
                        if (result is Act act)
                            foreach (var id in act.Identifiers.Where(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey)).ToArray())
                            {
                                act.Identifiers.Add(new ActIdentifier(id.Authority, this.m_hasher.ComputeHash(id.Value)));
                                act.Identifiers.Remove(id);
                                r++;
                            }
                        else if (result is Entity entity)
                            foreach (var id in entity.Identifiers.Where(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey)).ToArray())
                            {
                                entity.Identifiers.Add(new EntityIdentifier(id.Authority, this.m_hasher.ComputeHash(id.Value)));
                                entity.Identifiers.Remove(id);
                                r++;
                            }
                        if (r > 0)
                        {
                            AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid, PolicyGrantType.Deny)).ToList()), true, result);
                            if (result is ITaggable tag)
                            {
                                tag.AddTag("$pep.masked", "true");
                                tag.AddTag("$pep.method", "hash");
                            }
                        }
                        break;
                    }
                case ResourceDataPolicyActionType.Redact:
                case ResourceDataPolicyActionType.Redact | ResourceDataPolicyActionType.Audit:
                    {
                        var r = 0;
                        if (result is Act act)
                            foreach (var id in act.Identifiers.Where(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey)).ToArray())
                            {
                                act.Identifiers.Add(new ActIdentifier(id.Authority, new string('X', id.Value.Length)));
                                act.Identifiers.Remove(id);
                                r++;
                            }
                        else if (result is Entity entity)
                            foreach (var id in entity.Identifiers.Where(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey)).ToArray())
                            {
                                entity.Identifiers.Add(new EntityIdentifier(id.Authority, new string('X', id.Value.Length)));
                                entity.Identifiers.Remove(id);
                                r++;
                            }
                        if (r > 0)
                        {
                            AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid, PolicyGrantType.Deny)).ToList()), true, result);

                            if (result is ITaggable tag)
                            {
                                tag.AddTag("$pep.masked", "true");
                                tag.AddTag("$pep.method", "redact");
                            }
                        }
                        break;
                    }
                case ResourceDataPolicyActionType.Audit:

                    AuditUtil.AuditSensitiveDisclosure(result, null, true);
                    break;
            }
        }

        /// <summary>
        /// Validate that a query is not using restricted functions
        /// </summary>
        /// <typeparam name="TData">The type of data being queried</typeparam>
        /// <param name="query">The query expression</param>
        /// <param name="accessor">The user which is running the query</param>
        /// <returns>True if the user can execute the query, false if not</returns>
        public bool ValidateQuery<TModel>(Expression<Func<TModel, bool>> query, IPrincipal accessor) where TModel: IdentifiedData
        {
            if (accessor == AuthenticationContext.SystemPrincipal)
                return true;
            else
                return this.ValidateExpression(query, accessor, null);
        }

        /// <summary>
        /// Validate a LINQ expression for execution using <paramref name="policy"/>
        /// </summary>
        /// <param name="accessor">The principal which is running the query in which the <paramref name="expression"/> exists</param>
        /// <param name="expression">The expression which is being validated</param>
        /// <param name="policy">The policy configuration in scope for the query</param>
        /// <returns>True if <paramref name="accessor"/> can run the query part</returns>
        private bool ValidateExpression(Expression expression, IPrincipal accessor, ResourceDataPolicyFilter policy)
        {
            if(expression== null)
            {
                return true;
            }

            switch (expression)
            {
                case LambdaExpression le:
                    if (this.m_actions.TryGetValue(le.Parameters[0].Type, out var subPolicy))
                        return this.ValidateExpression(le.Body, accessor, subPolicy);
                    else
                        return true;
                case BinaryExpression be:
                    return this.ValidateExpression(be.Left, accessor, policy) && 
                        this.ValidateExpression(be.Right, accessor, policy);
                case UnaryExpression ue:
                    return this.ValidateExpression(ue.Operand, accessor, policy);
                case MemberExpression me:
                    
                    switch(me.Member)
                    {
                        case System.Reflection.PropertyInfo pi:
                            var serializationName = pi.GetSerializationName();
                            var fieldPolicy = policy.Fields?.FirstOrDefault(o => o.Property == serializationName);

                            if (fieldPolicy == null || fieldPolicy.Action == ResourceDataPolicyActionType.None)
                                return true;
                            else {
                                return fieldPolicy.Policy.Any() ? fieldPolicy.Policy.All(p => this.m_pdpService.GetPolicyOutcome(accessor, p) == PolicyGrantType.Grant) :
                                    fieldPolicy?.Action == ResourceDataPolicyActionType.None;
                            }
                        default:
                            return false;
                    }
                case MethodCallExpression mce:
                    return this.ValidateExpression(mce.Object, accessor, policy) &&
                        mce.Arguments.All(a => this.ValidateExpression(a, accessor, policy));
                case InvocationExpression ie:
                    return this.ValidateExpression(ie.Expression, accessor, policy);
                default:
                    return true;
            }
        }

        /// <summary>
        /// Returns true if updates to the record
        /// </summary>
        public bool ValidateWrite<TData>(TData record, IPrincipal accessor) where TData : IdentifiedData
        {
            // Is the record a bundle?
            if (record is Bundle bdl)
                return !bdl.Item.Any(o => !this.ValidateWrite(o, accessor)); // We do ! since we want the first FALSE to stop searching the bundle

            // Is this SYSTEM?
            if (!this.m_actions.TryGetValue(record.GetType(), out var policy) || accessor == AuthenticationContext.SystemPrincipal)
                return true;

            // Validate fields can be stored
            foreach (var itm in policy.Fields)
            {
                var value = record.GetType().GetQueryProperty(itm.Property)?.GetValue(record);
                if (value == null) continue;
                else
                {
                    return itm.Policy.Any() ? itm.Policy.All(p => this.m_pdpService.GetPolicyOutcome(accessor, p) != PolicyGrantType.Grant) :
                        itm.Action == ResourceDataPolicyActionType.None;
                }
            }

            if (policy.Action == ResourceDataPolicyActionType.None) // no enforcement
            {
                return true;
            }

            var decision = this.m_pdpService.GetPolicyDecision(accessor, record);
            if (decision.Outcome != PolicyGrantType.Grant)
                return false;
            else
            {
                var domainsToFilter = this.GetFilterDomains(accessor);
                if (record is Entity entity)
                    return !domainsToFilter.Any(dtf => entity.Identifiers.Any(id => id.Authority.SemanticEquals(dtf)));
                else if (record is Act act)
                    return !domainsToFilter.Any(dtf => act.Identifiers.Any(id => id.Authority.SemanticEquals(dtf)));
                else
                    return true;
            }
        }

        /// <summary>
        /// Apply the specified action
        /// </summary>
        public virtual TData Apply<TData>(TData result, IPrincipal principal) where TData : IdentifiedData
        {
            // Is the record a bundle?
            if (result == default(TData))
                return default(TData);
            else if (result is Bundle bdl)
            {
                bdl.Item = this.Apply(bdl.Item, principal).ToList(); // We do ! since we want the first FALSE to stop searching the bundle
                return result;
            }

            if (!this.m_actions.TryGetValue(result.GetType(), out var policy) || principal == AuthenticationContext.SystemPrincipal)
                return result;

            var decision = this.m_pdpService.GetPolicyDecision(principal, result);

            // First, apply identity security as that is independent
            this.ApplyIdentifierFilter(result, principal);

            // Apply the field filter
            this.ApplyFieldFilter(result, principal, policy);

            // Next we base on decision
            switch (decision.Outcome)
            {
                case PolicyGrantType.Elevate:
                case PolicyGrantType.Deny:
                    switch (policy.Action)
                    {
                        case ResourceDataPolicyActionType.Audit:
                            AuditUtil.AuditSensitiveDisclosure(result, decision, true);
                            return result;

                        case ResourceDataPolicyActionType.Hide:
                            return null;

                        case ResourceDataPolicyActionType.Hide | ResourceDataPolicyActionType.Audit:
                            AuditUtil.AuditMasking(result, decision, true, result);
                            return null;

                        case ResourceDataPolicyActionType.Redact:
                        case ResourceDataPolicyActionType.Redact | ResourceDataPolicyActionType.Audit:
                            {
                                if ((policy.Action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                                {
                                    AuditUtil.AuditMasking(result, decision, false, result);
                                }
                                result = (TData)this.MaskObject(result);
                                if (result is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                return result;
                            }
                        case ResourceDataPolicyActionType.Nullify:
                        case ResourceDataPolicyActionType.Nullify | ResourceDataPolicyActionType.Audit:
                            {
                                if ((policy.Action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                                {
                                    AuditUtil.AuditMasking(result, decision, true, result);
                                }

                                var nResult = Activator.CreateInstance(result.GetType()) as IdentifiedData;
                                nResult.Key = result.Key;
                                (nResult as IHasState).StatusConceptKey = StatusKeys.Nullified;
                                if (nResult is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                return (TData)nResult;
                            }
                        case ResourceDataPolicyActionType.Error:
                        case ResourceDataPolicyActionType.Error | ResourceDataPolicyActionType.Audit:
                            if ((policy.Action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                            {
                                AuditUtil.AuditSensitiveDisclosure(result, decision, false);
                            }
                            throw new SecurityException($"Access denied");
                        case ResourceDataPolicyActionType.None:
                            return result;

                        default:
                            throw new InvalidOperationException("Shouldn't be here - No Effective Policy Decision has been made");
                    }
                case PolicyGrantType.Grant:
                    if (result is ISecurable sec && sec.Policies.Any())
                        AuditUtil.AuditSensitiveDisclosure(result, decision, true);
                    return result;

                default:
                    throw new InvalidOperationException("Shouldn't be here - No Effective Policy Decision has been made");
            }
        }

        /// <summary>
        /// Filters any fields in the <paramref name="policy"/> according to their access information
        /// </summary>
        /// <typeparam name="TData">The type of data being filtered</typeparam>
        /// <param name="result">The result being filtered</param>
        /// <param name="principal">The principal being filtered</param>
        /// <param name="policy">The policy being applied</param>
        private void ApplyFieldFilter<TData>(TData result, IPrincipal principal, ResourceDataPolicyFilter policy) where TData : IdentifiedData
        {
            foreach (var itm in policy.Fields)
            {
                var property = result.GetType().GetQueryProperty(itm.Property);
                var value = property?.GetValue(result);
                if (value == null) continue;

                var hasPolicy = itm.Policy?.Select(p => this.m_pdpService.GetPolicyDecision(principal, p)).OrderBy(o => o.Outcome).FirstOrDefault();
                if (hasPolicy == null || hasPolicy.Outcome != PolicyGrantType.Grant)
                {
                    switch (policy.Action)
                    {
                        case ResourceDataPolicyActionType.Audit:
                            AuditUtil.AuditSensitiveDisclosure(result, hasPolicy, true, itm.Property);
                            break;
                        case ResourceDataPolicyActionType.Error:
                            if ((policy.Action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                            {
                                AuditUtil.AuditSensitiveDisclosure(result, hasPolicy, false, itm.Property);
                            }
                            throw new SecurityException($"Access denied");
                        case ResourceDataPolicyActionType.Nullify:
                        case ResourceDataPolicyActionType.Hide:
                            {
                                property.SetValue(result, null);
                                if (result is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                break;
                            }
                        case ResourceDataPolicyActionType.Redact:
                        case ResourceDataPolicyActionType.Redact | ResourceDataPolicyActionType.Audit:
                            {
                                if (typeof(String).IsAssignableFrom(property.PropertyType))
                                    property.SetValue(result, "XXXXX");
                                else
                                    property.SetValue(result, Activator.CreateInstance(property.PropertyType));
                                if (result is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                break;
                            }
                        default:
                            throw new InvalidOperationException("Cannot determine how to handle property instruction");
                    }
                }
            }
        }

        /// <summary>
        /// Mask the specified object
        /// </summary>
        private IdentifiedData MaskObject(IdentifiedData result)
        {
            if (result is Entity entity)
            {
                var retVal = Activator.CreateInstance(result.GetType()) as Entity;
                retVal.Key = result.Key;
                retVal.VersionKey = entity.VersionKey;
                retVal.VersionSequence = entity.VersionSequence;
                retVal.StatusConceptKey = entity.StatusConceptKey;
                retVal.AddTag("$pep.masked", "true");
                retVal.Policies = new List<SecurityPolicyInstance>(entity.Policies);
                retVal.StatusConceptKey = entity.StatusConceptKey;
                retVal.Names = entity.Names.Select(en => new EntityName(NameUseKeys.Anonymous, "XXXXX")).ToList();
                return retVal;
            }
            else if (result is Act act)
            {
                var retVal = Activator.CreateInstance(result.GetType()) as Act;
                retVal.Key = result.Key;
                retVal.VersionKey = act.VersionKey;
                retVal.VersionSequence = act.VersionSequence;
                retVal.StatusConceptKey = act.StatusConceptKey;
                retVal.AddTag("$pep.masked", "true");
                retVal.Policies = new List<SecurityPolicyInstance>(act.Policies);
                retVal.StatusConceptKey = act.StatusConceptKey;
                retVal.ReasonConceptKey = Guid.Parse("9b16bf12-073e-4ea4-b6c5-e1b93e8fd490");
                return retVal;
            }
            return result;
        }
    }
}