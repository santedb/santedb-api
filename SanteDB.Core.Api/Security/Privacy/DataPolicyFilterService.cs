/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace SanteDB.Core.Security.Privacy
{
    /// <summary>
    /// Local policy enforcement point service
    /// </summary>
    [ServiceProvider("Default Policy Enforcement Service")]
    public class DataPolicyFilterService : IDaemonService
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
        private Dictionary<IDataPersistenceService, KeyValuePair<Delegate, Delegate>> m_subscribedListeners = new Dictionary<IDataPersistenceService, KeyValuePair<Delegate, Delegate>>();

        // Protected authorities
        private List<AssigningAuthority> m_protectedAuthorities;

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
            var svcManager = ApplicationServiceContext.Current.GetService<IServiceManager>();

            this.m_tracer.TraceInfo("Starting bind to persistence services...");

            foreach (var t in typeof(Act).GetTypeInfo().Assembly.ExportedTypes.Where(o => typeof(Act).GetTypeInfo().IsAssignableFrom(o.GetTypeInfo()) || typeof(Entity).GetTypeInfo().IsAssignableFrom(o.GetTypeInfo())))
            {
                var svcType = typeof(IDataPersistenceService<>).MakeGenericType(t);
                var svcInstance = ApplicationServiceContext.Current.GetService(svcType);

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

                    this.m_subscribedListeners.Add(svcInstance as IDataPersistenceService, new KeyValuePair<Delegate, Delegate>(queriedInstanceDelegate, retrievedInstanceDelegate));
                }

            }

            // Bind to AA events
            var aaDp = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AssigningAuthority>>();
            this.m_protectedAuthorities = aaDp.Query(o => o.PolicyKey != null, AuthenticationContext.SystemPrincipal).ToList();
            // If we have a datacache then use that as it will get pubsub changes
            var dataCache = ApplicationServiceContext.Current.GetService<IDataCachingService>();
            if (dataCache != null)
            {
                dataCache.Added += (o, e) =>
                {
                    if ((e.Object as AssigningAuthority)?.PolicyKey.HasValue == true) this.m_protectedAuthorities.Add(e.Object as AssigningAuthority);
                };
                dataCache.Updated += (o, e) =>
                {
                    var data = e.Object as AssigningAuthority;
                    if (data?.PolicyKey.HasValue == true && !this.m_protectedAuthorities.Any(i => i.Key == data.Key))
                        this.m_protectedAuthorities.Add(data);
                    else if(data != null)
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
        }


        /// <summary>
        /// Handle post query event
        /// </summary>
        public virtual IEnumerable HandlePostQueryEvent(IEnumerable results)
        {
            
            // this is a very simple PEP which will enforce active policies in the result set.
            var pdp = ApplicationServiceContext.Current.GetService<IPolicyDecisionService>();

            // shall we get distinct AA for which we don't have permission to see
            var blockAa = this.m_protectedAuthorities
                .Where(aa => pdp.GetPolicyOutcome(AuthenticationContext.Current.Principal, aa.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid) != PolicyGrantType.Grant)
                .Select(aa => aa.Key)
                .ToArray();

            var decisions = results.OfType<Object>()
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(2)
                .Select(o=>new { Securable = o, Decision = pdp.GetPolicyDecision(AuthenticationContext.Current.Principal, o) });
            
            return decisions
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(2)
                // We want to mask ELEVATE
                .Where(o => o.Decision.Outcome != PolicyGrantType.Elevate && o.Securable is IdentifiedData).Select<dynamic, IdentifiedData>(
                    o => {
                        AuditUtil.AuditMasking(o.Securable as IdentifiedData, o.Decision == PolicyGrantType.Deny);

                        if (o.Decision == PolicyGrantType.Elevate)
                        {
                            if (o.Securable is Act)
                            {
                                return new Act()
                                {
                                    ReasonConceptKey = NullReasonKeys.Masked,
                                    Key = (o.Securable as IdentifiedData).Key
                                };
                            }
                            else if (o.Securable is Entity)
                                return new Entity() { Key = (o.Securable as IdentifiedData).Key };
                            else return null;
                        }
                        else
                            return null;
                    }
                )
                // We want to include all grant
                .Concat(
                    decisions.Where(o => o.Decision.Outcome == PolicyGrantType.Grant).Select(o => {
                        if (blockAa.Any())
                        {
                            (o.Securable as Act)?.Identifiers.RemoveAll(a => blockAa.Contains(a.AuthorityKey));
                            (o.Securable as Entity)?.Identifiers.RemoveAll(a => blockAa.Contains(a.AuthorityKey));
                        }
                        return o.Securable;
                    })
                )
                .ToList();
        }


        /// <summary>
        /// Handle post query event
        /// </summary>
        public virtual Object HandlePostRetrieveEvent(Object result)
        {
            // this is a very simple PEP which will enforce active policies in the result set.
            var pdp = ApplicationServiceContext.Current.GetService<IPolicyDecisionService>();

            // shall we get distinct AA for which we don't have permission to see
            var blockAa = this.m_protectedAuthorities
                .Where(aa => pdp.GetPolicyOutcome(AuthenticationContext.Current.Principal, aa.LoadProperty<SecurityPolicy>(nameof(AssigningAuthority.Policy)).Oid) != PolicyGrantType.Grant)
                .Select(aa => aa.Key)
                .ToArray();

            var decision = pdp.GetPolicyDecision(AuthenticationContext.Current.Principal, result);
            var iResult = result as IdentifiedData;

            // Get rid of identifiers
            (result as Act)?.Identifiers.RemoveAll(a => blockAa.Contains(a.AuthorityKey));
            (result as Entity)?.Identifiers.RemoveAll(a => blockAa.Contains(a.AuthorityKey));

            // Decision outcome
            switch (decision.Outcome)
            {
                case PolicyGrantType.Deny:
                    AuditUtil.AuditMasking(iResult, true);
                    return null;
                case PolicyGrantType.Elevate:
                    AuditUtil.AuditMasking(iResult, false);
                    if (result is Act)
                    {
                        return new Act()
                        {
                            ReasonConceptKey = NullReasonKeys.Masked,
                            Key = iResult.Key
                        };
                    }
                    else
                        return new Entity() { Key = iResult.Key };
                default:
                    return result;
            }

        }

        /// <summary>
        /// Removes bindings for the specified events
        /// </summary>
        private void UnBindEvents()
        {
            foreach(var i in this.m_subscribedListeners)
            {
                i.Key.GetType().GetRuntimeEvent("Queried").RemoveEventHandler(i.Key, i.Value.Key);
                i.Key.GetType().GetRuntimeEvent("Retrieved").RemoveEventHandler(i.Key, i.Value.Value);
            }
        }

    }
}
