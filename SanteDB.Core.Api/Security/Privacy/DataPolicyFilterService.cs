/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Interfaces;
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
using SanteDB.Core.Services.Impl;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading.Tasks;

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
        private ConcurrentDictionary<Type, ResourceDataPolicyActionType> m_actions = new ConcurrentDictionary<Type, ResourceDataPolicyActionType>();

        // PDP Service
        private IPolicyDecisionService m_pdpService;
        // Adhoc cache
        private IAdhocCacheService m_adhocCache;
        // Password Hashing
        private IPasswordHashingService m_hasher;
        // Threadpool service
        private IThreadPoolService m_threadPool;

        /// <summary>
        /// Data policy filter service with DI
        /// </summary>
        public DataPolicyFilterService(IConfigurationManager configurationManager, IPasswordHashingService passwordService, IPolicyDecisionService pdpService, IThreadPoolService threadPoolService,
             IAdhocCacheService adhocCache = null)
        {
            this.m_hasher = passwordService;
            this.m_adhocCache = adhocCache;
            this.m_pdpService = pdpService;
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
                    if (typeof(Act).IsAssignableFrom(t.ResourceType) || typeof(Entity).IsAssignableFrom(t.ResourceType))
                    {
                        this.m_tracer.TraceInfo("Binding privacy action {0} to {1}", t.Action, t.ResourceType);
                        this.m_actions.TryAdd(t.ResourceType, t.Action);
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
            if (!this.m_actions.TryGetValue(typeof(AssigningAuthority), out ResourceDataPolicyActionType action) && !this.m_actions.TryGetValue(result.GetType(), out action))
                action = this.m_configuration.DefaultAction;
            var domainsToFilter = this.GetFilterDomains(accessor);

            switch (action)
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
                            if ((action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                            {
                                AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid, PolicyGrantType.Deny)).ToList()), true);
                            }

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
                            if ((action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                            {
                                AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid, PolicyGrantType.Deny)).ToList()), true);
                            }
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
        /// Returns true if updates to the record 
        /// </summary>
        public bool ValidateWrite<TData>(TData record, IPrincipal accessor) where TData : IdentifiedData
        {

            // Is the record a bundle?
            if (record is Bundle bdl)
                return !bdl.Item.Any(o => !this.ValidateWrite(o, accessor)); // We do ! since we want the first FALSE to stop searching the bundle

            // Is this SYSTEM?
            if (!this.m_actions.TryGetValue(record.GetType(), out ResourceDataPolicyActionType action) || accessor == AuthenticationContext.SystemPrincipal || action == ResourceDataPolicyActionType.None)
                return true;

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

            if (!this.m_actions.TryGetValue(result.GetType(), out ResourceDataPolicyActionType action) || principal == AuthenticationContext.SystemPrincipal || action == ResourceDataPolicyActionType.None)
                return result;

            var decision = this.m_pdpService.GetPolicyDecision(principal, result);

            // First, apply identity security as that is independent
            this.ApplyIdentifierFilter(result, principal);

            // Next we base on decision
            switch (decision.Outcome)
            {
                case PolicyGrantType.Elevate:
                case PolicyGrantType.Deny:
                    switch (action)
                    {
                        case ResourceDataPolicyActionType.Audit:
                            AuditUtil.AuditSensitiveDisclosure(result, decision, true);
                            return result;
                        case ResourceDataPolicyActionType.Hide:
                            return null;
                        case ResourceDataPolicyActionType.Hide | ResourceDataPolicyActionType.Audit:
                            AuditUtil.AuditMasking(result, decision, true);
                            return null;
                        case ResourceDataPolicyActionType.Redact:
                        case ResourceDataPolicyActionType.Redact | ResourceDataPolicyActionType.Audit:
                            {

                                if ((action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                                {
                                    AuditUtil.AuditMasking(result, decision, false);
                                }
                                result = (TData)this.MaskObject(result);
                                if (result is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                return result;
                            }
                        case ResourceDataPolicyActionType.Nullify:
                        case ResourceDataPolicyActionType.Nullify | ResourceDataPolicyActionType.Audit:
                            {
                                if ((action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
                                {
                                    AuditUtil.AuditMasking(result, decision, true);
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
                            if ((action & ResourceDataPolicyActionType.Audit) == ResourceDataPolicyActionType.Audit)
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
