﻿/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using SanteDB.Core.Data;
using SanteDB.Core.Http;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Notifications;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Principal;
using System.Reflection;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Configuration;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using SanteDB.Core.i18n;
using System.Collections;
using SanteDB.Core.Model.Collection;

namespace SanteDB.Core
{
    /// <summary>
    /// Extension methods for the core API functions
    /// </summary>
    public static class ExtensionMethods
    {

        // Verified assemblies
        private static readonly ConcurrentBag<String> m_verifiedAssemblies = new ConcurrentBag<string>();

        // Lambda from the message property
        private static readonly Regex m_formatRegex = new Regex(@"\{\{(.+?)\}\}", RegexOptions.Compiled);

        /// <summary>
        /// Create injected class
        /// </summary>
        public static object CreateInjected(this Type me)
        {
            return ApplicationServiceContext.Current.GetService<IServiceManager>().CreateInjected(me);
        }

        /// <summary>
        /// Resolve the managed target wrapper for <see cref="IDataManagedLinkProvider{T}.ResolveManagedRecord(T)"/>
        /// </summary>
        public static T ResolveManagedRecord<T>(this T forSource) where T : IdentifiedData =>
            ApplicationServiceContext.Current.GetService<IDataManagementPattern>()?.GetLinkProvider<T>()?.ResolveManagedRecord(forSource) ?? forSource;

        /// <summary>
        /// Resolve the managed target wrapper for <see cref="IDataManagedLinkProvider{T}.ResolveOwnedRecord(T, IPrincipal)"/>
        /// </summary>
        public static T ResolveOwnedRecord<T>(this T forSource, IPrincipal ownerPrincipal) where T : IdentifiedData =>
            ApplicationServiceContext.Current.GetService<IDataManagementPattern>()?.GetLinkProvider<T>()?.ResolveOwnedRecord(forSource, ownerPrincipal) ?? forSource;

        /// <summary>
        /// Non generic method of <see cref="ResolveOwnedRecord{T}(T, IPrincipal)"/>
        /// </summary>
        public static IdentifiedData ResolveOwnedRecord(this IdentifiedData forSource, IPrincipal ownerPrincipal)
            => ApplicationServiceContext.Current.GetService<IDataManagementPattern>()?.GetLinkProvider(forSource.GetType())?.ResolveOwnedRecord(forSource, ownerPrincipal) ?? forSource;

        /// <summary>
        /// Get managed reference links wrapper for <see cref="IDataManagedLinkProvider{T}.FilterManagedReferenceLinks(IEnumerable{ITargetedAssociation})"/>
        /// </summary>
        public static IEnumerable<ITargetedAssociation> FilterManagedReferenceLinks<T>(this IEnumerable<Model.Association<T>> forRelationships) where T : IdentifiedData, IHasClassConcept, IHasTypeConcept, IAnnotatedResource, IHasRelationships, new() =>
            ApplicationServiceContext.Current.GetService<IDataManagementPattern>()?.GetLinkProvider<T>()?.FilterManagedReferenceLinks(forRelationships.OfType<ITargetedAssociation>()) ?? forRelationships.Where(o => false).OfType<ITargetedAssociation>();

        /// <summary>
        /// Add a managed reference link between <paramref name="sourceObject"/> and <paramref name="targetObject"/>
        /// </summary>
        public static ITargetedAssociation AddManagedReferenceLink<T>(this T sourceObject, T targetObject) where T : IdentifiedData =>
            ApplicationServiceContext.Current.GetService<IDataManagementPattern>()?.GetLinkProvider<T>()?.AddManagedReferenceLink(sourceObject, targetObject) ?? null;


        /// <summary>
        /// Returns true if the job schedule applies at <paramref name="refDate"/> given the <paramref name="lastRun"/>
        /// </summary>
        /// <param name="me">The job schedule to determine applicability</param>
        /// <param name="refDate">The time that the system is checking if the job execution applies</param>
        /// <param name="lastRun">The last known run time / check time of the job. Null if never run</param>
        /// <returns>True if the schedule applies</returns>
        public static bool AppliesTo(this IJobSchedule me, DateTime refDate, DateTime? lastRun)
        {
            var retVal = refDate >= me.StartTime; // The reference date is in valid bounds for start
            retVal &= !me.StopTime.HasValue || refDate < me.StopTime.Value; // The reference date is in valid bounds of stop (if specified)

            // Are there week days specified
            if (me.Type == Configuration.JobScheduleType.Interval && (!lastRun.HasValue || refDate.Subtract(lastRun.Value) > me.Interval))
            {
                return true;
            }
            else if (me.Type == Configuration.JobScheduleType.Scheduled)
            {
                if (me.Days != null && me.Days.Any())
                {
                    retVal &= me.Days.Any(r => r == refDate.DayOfWeek) &&
                        refDate.Hour >= me.StartTime.Hour &&
                        refDate.Minute >= me.StartTime.Minute &&
                        refDate.Date > me.StartTime;
                    retVal &= !lastRun.HasValue ? DateTime.Now.Hour == me.StartTime.Hour : (lastRun.Value.Date < refDate.Date); // Last run does not cover this calculation - i.e. have we not already run this repeat?
                }
                else // This is an exact time
                {
                    retVal &= refDate.Date == me.StartTime.Date &&
                        refDate.Hour >= me.StartTime.Hour &&
                        refDate.Minute >= me.StartTime.Minute &&
                        !lastRun.HasValue;
                }
            }

            return retVal;

        }

        /// <summary>
        /// Determine if the <see cref="IJobState"/> is running
        /// </summary>
        /// <param name="me">The job state structure</param>
        /// <returns>True if the status of the job state implies the job is running</returns>
        public static bool IsRunning(this IJobState me) => me.CurrentState == JobStateType.Running || me.CurrentState == JobStateType.Starting;

        /// <summary>
		/// Get application provider service
		/// </summary>
		/// <param name="me">The current application context.</param>
		/// <returns>Returns an instance of the <see cref="IApplicationIdentityProviderService"/>.</returns>
		public static IApplicationIdentityProviderService GetApplicationProviderService(this IApplicationServiceContext me)
        {
            return me.GetService<IApplicationIdentityProviderService>();
        }

        /// <summary>
        /// Gets the assigning authority repository service.
        /// </summary>
        /// <param name="me">The current application context.</param>
        /// <returns>Returns an instance of the <see cref="IIdentityDomainRepositoryService"/>.</returns>
        public static IIdentityDomainRepositoryService GetAssigningAuthorityService(this IApplicationServiceContext me)
        {
            return me.GetService<IIdentityDomainRepositoryService>();
        }

        /// <summary>
        /// Gets the business rules service for a specific information model.
        /// </summary>
        /// <typeparam name="T">The type of information for which to retrieve the business rules engine instance.</typeparam>
        /// <param name="me">The application context.</param>
        /// <returns>Returns an instance of the business rules service.</returns>
        public static IBusinessRulesService<T> GetBusinessRulesService<T>(this IApplicationServiceContext me) where T : IdentifiedData
        {
            return me.GetService<IBusinessRulesService<T>>();
        }

        /// <summary>
        /// Get the concept service.
        /// </summary>
        /// <param name="me">The current application context.</param>
        /// <returns>Returns an instance of the <see cref="IConceptRepositoryService"/>.</returns>
        public static IConceptRepositoryService GetConceptService(this IApplicationServiceContext me)
        {
            return me.GetService<IConceptRepositoryService>();
        }

        /// <summary>
        /// Gets the <see cref="INotificationService"/> instance from <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The service context to get the <see cref="INotificationService"/> from.</param>
        /// <returns>The <see cref="INotificationService"/> in the <paramref name="context"/>.</returns>
        public static INotificationService GetNotificationService(this IApplicationServiceContext context)
            => context.GetService<INotificationService>();

        /// <summary>
        /// Get the audit service.
        /// </summary>
        /// <param name="me">The application context.</param>
        /// <returns>Returns an instance of the <see cref="IAuditService"/>.</returns>
        public static IAuditService GetAuditService(this IApplicationServiceContext me) => me.GetService<IAuditService>();

        /// <summary>
        /// Gets the user identifier for a given identity.
        /// </summary>
        /// <returns>Returns a string which represents the users identifier, or null if unable to retrieve the users identifier.</returns>
        public static string GetUserId(IIdentity source)
        {
            return GetUserId<string>(source);
        }

        /// <summary>
        /// Gets the user identifier for a given identity.
        /// </summary>
        /// <typeparam name="T">The type of the identifier of the user.</typeparam>
        /// <returns>Returns the users identifier, or null if unable to retrieve the users identifier.</returns>
        public static T GetUserId<T>(IIdentity source) where T : IConvertible
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Value cannot be null");
            }

            var userId = default(T);

            var nameIdentifierClaimValue = (source as IClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == SanteDBClaimTypes.NameIdentifier)?.Value;

            if (nameIdentifierClaimValue != null)
            {
                userId = (T)Convert.ChangeType(nameIdentifierClaimValue, typeof(T), CultureInfo.InvariantCulture);
            }

            return userId;
        }

        /// <summary>
        /// Convert to policy instance
        /// </summary>
        public static SecurityPolicyInstance ToPolicyInstance(this IPolicyInstance me)
        {
            return new SecurityPolicyInstance(
                new SecurityPolicy()
                {
                    CanOverride = me.Policy.CanOverride,
                    Oid = me.Policy.Oid,
                    Name = me.Policy.Name
                },
                (PolicyGrantType)(int)me.Rule
            );
        }

        /// <summary>
        /// Tries to dequeue a message from the dispatcher queue. Returns <c>true</c> if successful, <c>false</c> otherwise.
        /// </summary>
        /// <param name="svc">The service implementation to dequeue from.</param>
        /// <param name="queueName">The name of the queue to dequeue from.</param>
        /// <param name="queueEntry">Out; the entry that was dequeued.</param>
        /// <returns>True if the operation succeeded, false otherwise.</returns>
        public static bool TryDequeue(this IDispatcherQueueManagerService svc, string queueName, out DispatcherQueueEntry queueEntry)
        {
            var entry = svc.Dequeue(queueName);
            queueEntry = entry;
            return null != entry;
        }

        /// <summary>
        /// Set timeout on <paramref name="me"/> to <paramref name="millisecondTimeout"/>
        /// </summary>
        /// <param name="me">The rest client to set the timeout  on</param>
        /// <param name="millisecondTimeout">The timeout to set</param>
        public static void SetTimeout(this IRestClient me, int millisecondTimeout)
        {
            me.Description.Endpoint.ForEach(o => { o.Timeout = new TimeSpan(0, 0, 0, 0, millisecondTimeout); });
        }

        /// <summary>
        /// Validate that the code is signed
        /// </summary>
        public static void ValidateCodeIsSigned(this Assembly asm, bool allowUnsignedAssemblies)
        {
            var tracer = Tracer.GetTracer(typeof(ExtensionMethods));
            bool valid = false;
            var asmFile = asm.Location;
            if (String.IsNullOrEmpty(asmFile))
            {
                tracer.TraceWarning("Cannot verify {0} - no assembly location found", asmFile);
            }
            else if (allowUnsignedAssemblies != true)
            {
                // Verified assembly?
                if (!m_verifiedAssemblies.Contains(asmFile))
                {
                    try
                    {
                        var extraCerts = new X509Certificate2Collection();
                        extraCerts.Import(asmFile);

                        var certificate = new X509Certificate2(X509Certificate2.CreateFromSignedFile(asmFile));
                        tracer.TraceVerbose("Validating {0} published by {1}", asmFile, certificate.Subject);
                        valid = certificate.IsTrustedIntern(extraCerts, out IEnumerable<X509ChainStatus> chainStatus);
                        if (!valid)
                        {
                            throw new SecurityException($"File {asmFile} published by {certificate.Subject} is not trusted in this environment ({String.Join(",", chainStatus.Select(o => $"{o.Status}:{o.StatusInformation}"))})");
                        }
                    }
                    catch (Exception e)
                    {
#if !DEBUG
                        throw new SecurityException($"Could not load digital signature information for {asmFile}", e);
#else
                        tracer.TraceWarning("Could not verify {0} due to error {1}", asmFile, e.Message);
                        valid = false;
#endif
                    }
                }
                else
                {
                    valid = true;
                }

                if (!valid)
                {
#if !DEBUG
                    throw new SecurityException($"Assembly {asmFile} is not signed - or its signature could not be validated! Plugin may be tampered!");
#else
                    m_verifiedAssemblies.Add(asmFile);
                    tracer.TraceWarning("!!!!!!!!! ALERT !!!!!!! {0} in {1} is not signed - in a release version of SanteDB this will cause the host to not load this service!", asm, asmFile);
#endif
                }
                else
                {
                    tracer.TraceVerbose("{0} was validated as trusted code", asmFile);
                    m_verifiedAssemblies.Add(asmFile);
                }
            }
        }


        /// <summary>
        /// Get a string formatted to the message with the specified input object
        /// </summary>
        /// <param name="objectData">The parameter data</param>
        /// <param name="template">The formatting string</param>
        public static String FormatString(this String template, object objectData)
        {
            if(objectData is IDictionary<String, Object> dict)
            {
                if(String.IsNullOrEmpty(template))
                {
                    return String.Join(",", dict.Values);
                }
                else
                {
                    return m_formatRegex.Replace(template.Trim(), o => dict.TryGetValue(o.Groups[1].Value.Trim(), out var v) ? v.ToString() : String.Empty);
                }
            }
            else if (objectData.GetType().GetMember("get_Item") != null)
            {
                var getMember = objectData.GetType().GetMethod("get_Item");
                return m_formatRegex.Replace(template.Trim(), o =>
                {
                    return getMember.Invoke(objectData, new object[] { o.Groups[1].Value.Trim() })?.ToString();
                });
            }
            else
            {
                throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(IDictionary), objectData.GetType()));
            }
        }

    }
}