/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2020-5-1
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
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

namespace SanteDB.Core.Security.Privacy
{
    /// <summary>
    /// Local policy enforcement point service
    /// </summary>
    [ServiceProvider("Default Policy Enforcement Service")]
    public class DataPolicyFilterService : IDaemonService, IPrivacyEnforcementService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Data Policy Enforcement Filter Service";

        // Set to true when the application context has stopped
        private bool m_safeToStop = false;

        // Security tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataPolicyFilterService));

        // Subscribed listeners
        private Dictionary<Object, KeyValuePair<Delegate, Delegate>> m_subscribedListeners = new Dictionary<Object, KeyValuePair<Delegate, Delegate>>();

        // Filter configuration
        private DataPolicyFilterConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<DataPolicyFilterConfigurationSection>();

        // Actions
        private ConcurrentDictionary<Type, ResourceDataPolicyActionType> m_actions = new ConcurrentDictionary<Type, ResourceDataPolicyActionType>();

        // Protected authorities
        private List<AssigningAuthority> m_protectedAuthorities;
        // PDP Service
        private IPolicyDecisionService m_pdpService;
        // Adhoc cache
        private IAdhocCacheService m_adhocCache ;
        // Password Hashing
        private IPasswordHashingService m_hasher;
        // Data caching service
        private IDataCachingService m_dataCachingService;
        // Subscription executor
        private ISubscriptionExecutor m_subscriptionExecutor;

        /// <summary>
        /// Data policy filter service with DI
        /// </summary>
        public DataPolicyFilterService(IPasswordHashingService passwordService, IPolicyDecisionService pdpService,
            IDataCachingService dataCachingService, ISubscriptionExecutor subscriptionExecutor = null, IAdhocCacheService adhocCache = null)
        {
            this.m_hasher = passwordService;
            this.m_adhocCache = adhocCache;
            this.m_pdpService = pdpService;
            this.m_subscriptionExecutor = subscriptionExecutor;
            this.m_dataCachingService = dataCachingService;
        }

        /// <summary>
        /// Determines whether the service is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.m_subscribedListeners.Count > 0;
            }
        }

        /// <summary>
        /// The service is started
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// The service is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// The service is stopped
        /// </summary>
        public event EventHandler Stopped;
        /// <summary>
        /// The service is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Starts the service
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started += (o, e) => this.BindEvents();

            this.Started?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Stop the policy enforcement service
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            // Audit tool should never stop!!!!!
            if (!this.m_safeToStop)
                this.UnBindEvents();

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Binds the specified events
        /// </summary>
        private void BindEvents()
        {
            this.m_tracer.TraceInfo("Starting bind to persistence services...");
            var policyTypes = this.m_configuration?.Resources?.Select(o => o.ResourceType) ?? typeof(Act).GetTypeInfo().Assembly.ExportedTypes.Where(o => typeof(Act).GetTypeInfo().IsAssignableFrom(o.GetTypeInfo()) || typeof(Entity).GetTypeInfo().IsAssignableFrom(o.GetTypeInfo()));
            foreach (var t in policyTypes)
            {
                var svcType = typeof(INotifyRepositoryService<>).MakeGenericType(t);
                var svcInstance = ApplicationServiceContext.Current.GetService(svcType);
                var action = this.m_configuration?.Resources.FirstOrDefault(o => o.ResourceType == t)?.Action ?? this.m_configuration?.DefaultAction ?? ResourceDataPolicyActionType.Hide;
                this.m_actions.TryAdd(t, action);
                if (action == ResourceDataPolicyActionType.None) continue; // no action

                // Now comes the tricky dicky part - We need to subscribe to a generic event
                if (svcInstance != null)
                {
                    this.m_tracer.TraceInfo("Binding to {0}...", svcType);

                    // Construct the delegate for query
                    var pqeArgType = typeof(QueryResultEventArgs<>).MakeGenericType(t);
                    var qevtHdlrType = typeof(EventHandler<>).MakeGenericType(pqeArgType);
                    var senderParm = Expression.Parameter(typeof(Object), "o");
                    var eventParm = Expression.Parameter(pqeArgType, "e");
                    var delegateData = Expression.Convert(Expression.MakeMemberAccess(eventParm, pqeArgType.GetRuntimeProperty("Results")), typeof(IEnumerable));
                    var ofTypeMethod = typeof(Enumerable).GetGenericMethod(nameof(Enumerable.OfType), new Type[] { t }, new Type[] { typeof(IEnumerable) }) as MethodInfo;
                    var queriedInstanceDelegate = Expression.Lambda(qevtHdlrType, Expression.Assign(delegateData.Operand, Expression.Convert(Expression.Call(ofTypeMethod, Expression.Call(Expression.Constant(this), typeof(DataPolicyFilterService).GetRuntimeMethod(nameof(HandlePostQueryEvent), new Type[] { typeof(IEnumerable) }), delegateData)), delegateData.Operand.Type)), senderParm, eventParm).Compile();

                    // Bind to events
                    svcType.GetRuntimeEvent("Queried").AddEventHandler(svcInstance, queriedInstanceDelegate);

                    // Construct delegate for retrieve
                    pqeArgType = typeof(DataRetrievedEventArgs<>).MakeGenericType(t);
                    qevtHdlrType = typeof(EventHandler<>).MakeGenericType(pqeArgType);
                    senderParm = Expression.Parameter(typeof(Object), "o");
                    eventParm = Expression.Parameter(pqeArgType, "e");
                    delegateData = Expression.Convert(Expression.MakeMemberAccess(eventParm, pqeArgType.GetRuntimeProperty("Data")), t);
                    var retrievedInstanceDelegate = Expression.Lambda(qevtHdlrType, Expression.Assign(delegateData.Operand, Expression.Convert(Expression.Call(Expression.Constant(this), typeof(DataPolicyFilterService).GetRuntimeMethod(nameof(HandlePostRetrieveEvent), new Type[] { t }), delegateData), t)), senderParm, eventParm).Compile();

                    // Bind to events
                    svcType.GetRuntimeEvent("Retrieved").AddEventHandler(svcInstance, retrievedInstanceDelegate);

                    this.m_subscribedListeners.Add(svcInstance, new KeyValuePair<Delegate, Delegate>(queriedInstanceDelegate, retrievedInstanceDelegate));
                }

            }

            // Bind to AA events
            var aaDp = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AssigningAuthority>>();
            this.m_protectedAuthorities = aaDp?.Query(o => o.PolicyKey != null, AuthenticationContext.SystemPrincipal).ToList();
            // If we have a datacache then use that as it will get pubsub changes
            if (this.m_dataCachingService != null)
            {
                this.m_dataCachingService.Added += (o, e) =>
                {
                    if ((e.Object as AssigningAuthority)?.PolicyKey.HasValue == true) this.m_protectedAuthorities.Add(e.Object as AssigningAuthority);
                };
                this.m_dataCachingService.Updated += (o, e) =>
                {
                    var data = e.Object as AssigningAuthority;
                    if (data?.PolicyKey.HasValue == true && !this.m_protectedAuthorities.Any(i => i.Key == data.Key))
                        this.m_protectedAuthorities.Add(data);
                    else if (data != null)
                        this.m_protectedAuthorities.RemoveAll(i => i.Key == data.Key);
                };
            }
            else
            {
                aaDp.Inserted += (o, e) =>
                {
                    if (e.Data.PolicyKey.HasValue) this.m_protectedAuthorities.Add(e.Data);
                };
                aaDp.Updated += (o, e) =>
                {
                    if (e.Data.PolicyKey.HasValue && !this.m_protectedAuthorities.Any(i => i.Key == e.Data.Key))
                        this.m_protectedAuthorities.Add(e.Data);
                    else
                        this.m_protectedAuthorities.RemoveAll(i => i.Key == e.Data.Key);
                };
            }

            if (this.m_subscriptionExecutor != null)
                this.m_subscriptionExecutor.Executed += (o, e) =>
                {
                    e.Results = this.HandlePostQueryEvent(e.Results).OfType<IdentifiedData>();
                };
        }


        /// <summary>
        /// Handle post query event
        /// </summary>
        public virtual IEnumerable HandlePostQueryEvent(IEnumerable results)
        {

            var principal = AuthenticationContext.Current.Principal;
            return results
                .OfType<IdentifiedData>()
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(2)
                .Select(
                    o => this.Apply(o, principal)
                )
                .ToList();
        }


        /// <summary>
        /// Handle post query event
        /// </summary>
        public virtual Object HandlePostRetrieveEvent(Object result)
        {
            if (result == null) return null;
            // Get rid of identifiers
            return this.Apply(result as IdentifiedData, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Apply actions to hid identity
        /// </summary>
        private void ApplyIdentifierFilter(IdentifiedData result, IPrincipal accessor)
        {
            this.m_actions.TryGetValue(result.GetType(), out ResourceDataPolicyActionType action);

            String key = null;
            if (accessor is IClaimsPrincipal cp && cp.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBSessionIdClaim))
                key = this.m_hasher.ComputeHash($"$aa.filter.{cp.FindFirst(SanteDBClaimTypes.SanteDBSessionIdClaim).Value}");
            else
                key = this.m_hasher.ComputeHash($"$aa.filter.{accessor.Identity.Name}");

            var domainsToFilter = this.m_adhocCache?.Get<AssigningAuthority[]>(key);
            if (domainsToFilter == null)
            {
                domainsToFilter = this.m_protectedAuthorities
                        .Where(aa => this.m_pdpService.GetPolicyOutcome(AuthenticationContext.Current.Principal, aa.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid) != PolicyGrantType.Grant)
                        .ToArray();
                this.m_adhocCache?.Add(key, domainsToFilter);
            }

            switch (action)
            {
                case ResourceDataPolicyActionType.Hide:
                case ResourceDataPolicyActionType.Nullify:
                    {
                        var r = (result as Act)?.Identifiers.RemoveAll(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey));
                        r += (result as Entity)?.Identifiers.RemoveAll(a => domainsToFilter.Any(f => f.Key == a.AuthorityKey));
                        if (r > 0)
                        {
                            AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.Policy.Oid, PolicyGrantType.Deny)).ToList()), true);
                            if (result is ITaggable tag)
                                tag.AddTag("$pep.masked", "true");
                        }
                            break;
                    }
                case ResourceDataPolicyActionType.Redact:
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
                            AuditUtil.AuditMasking(result, new PolicyDecision(result, domainsToFilter.Select(o => new PolicyDecisionDetail(o.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid, PolicyGrantType.Deny)).ToList()), true);
                            if (result is ITaggable tag)
                                tag.AddTag("$pep.masked", "true");
                        }
                        break;
                    }
                case ResourceDataPolicyActionType.Audit:
                    // TODO:
                    break;

            }

        }

        /// <summary>
        /// Apply the specified action
        /// </summary>
        public IdentifiedData Apply(IdentifiedData result, IPrincipal accessor)
        {
            var decision = this.m_pdpService.GetPolicyDecision(accessor, result);
            this.m_actions.TryGetValue(result.GetType(), out ResourceDataPolicyActionType action);

            // First, apply identity security as that is independent
            this.ApplyIdentifierFilter(result, accessor);

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
                            AuditUtil.AuditMasking(result, decision, true);
                            return null;
                        case ResourceDataPolicyActionType.Redact:
                            {

                                AuditUtil.AuditMasking(result, decision, false);
                                result = this.MaskObject(result);
                                if (result is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                return result;
                            }
                        case ResourceDataPolicyActionType.Nullify:
                            {

                                AuditUtil.AuditMasking(result, decision, true);
                                var nResult = Activator.CreateInstance(result.GetType()) as IdentifiedData;
                                nResult.Key = result.Key;
                                (nResult as IHasState).StatusConceptKey = StatusKeys.Nullified;
                                if (nResult is ITaggable tag)
                                    tag.AddTag("$pep.masked", "true");
                                return nResult;
                            }
                        case ResourceDataPolicyActionType.Error:
                            AuditUtil.AuditSensitiveDisclosure(result, decision, false);
                            throw new SecurityException($"Access denied");
                        case ResourceDataPolicyActionType.None:
                            return result;
                        default:
                            throw new InvalidOperationException("Shouldn't be here - No Effective Policy Decision has been made");
                    }
                    break;
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

        /// <summary>
        /// Removes bindings for the specified events
        /// </summary>
        private void UnBindEvents()
        {
            foreach (var i in this.m_subscribedListeners)
            {
                i.Key.GetType().GetRuntimeEvent("Queried").RemoveEventHandler(i.Key, i.Value.Key);
                i.Key.GetType().GetRuntimeEvent("Retrieved").RemoveEventHandler(i.Key, i.Value.Value);
            }
        }

    }
}
