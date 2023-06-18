/*
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
 * Date: 2023-5-19
 */
using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Queue;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Audit
{

    /// <summary>
    /// The default implementation of the audit service.
    /// </summary>
    [ServiceProvider(SERVICE_NAME)]
    public class DefaultAuditService : IAuditService
    {
        private const string SERVICE_NAME = "Default Audit Service";

        static readonly string s_ProcessId = Process.GetCurrentProcess().Id.ToString();
        static readonly string s_ProcessName = Process.GetCurrentProcess().ProcessName;

        /// <inheritdoc/>
        public string ServiceName => SERVICE_NAME;

        readonly IRepositoryService<AuditEventData> _AuditRepository; //Local DB.
        readonly AuditAccountabilityConfigurationSection _Configuration;
        readonly IDispatcherQueueManagerService _QueueService; //File, MSMQ, RabbitMq
        readonly IAuditDispatchService _Dispatcher; //Atna, Fhir, Etc.

        readonly Tracer _Tracer;


        /// <summary>
        /// DI Constructor
        /// </summary>
        public DefaultAuditService(IServiceProvider serviceProvider)
        {
            _Tracer = new Tracer(nameof(DefaultAuditService));
            _AuditRepository = serviceProvider.GetService<IRepositoryService<AuditEventData>>();
            _QueueService = serviceProvider.GetService<IDispatcherQueueManagerService>();
            _Dispatcher = serviceProvider.GetService<IAuditDispatchService>();

            var configurationmanager = serviceProvider.GetService<IConfigurationManager>();

            _Configuration = configurationmanager.GetSection<AuditAccountabilityConfigurationSection>();

            if (null == _QueueService)
            {
                _Tracer.TraceInfo("No {0} is registered in the service provider. Sending audit data will be synchronous. Consider adding an implementation of {0} to the service context.", nameof(IDispatcherQueueManagerService));


            }
        }

        /// <inheritdoc />
        public IAuditBuilder Audit()
        {
            _Tracer.TraceVerbose("Creating Audit Builder");
            return new AuditServiceAuditEventData(this);
        }

        /// <inheritdoc />
        public void SendAudit(AuditEventData audit)
        {
            try
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    if (null != _QueueService)
                    {
                        _QueueService.Enqueue(AuditConstants.QueueName, audit);
                    }
                    else
                    {
                        //Dispatch synchronously.
                        DispatchAudit(audit);
                    }
                }
            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            {
                _Tracer.TraceError("Error sending audit to queue: {0}", ex);
                throw;
            }
        }

        /// <summary>
        /// Populates key information in the audit event just before sending it. This should always be the last step in an audit entry builder.
        /// </summary>
        /// <param name="audit"></param>
        protected virtual void PrepareAuditForSend(AuditEventData audit)
        {
            try
            {
                var rc = RemoteEndpointUtil.Current.GetRemoteClient();
                var principal = AuthenticationContext.Current.Principal as IClaimsPrincipal;

                // Get audit metadata
                audit.AddMetadata(AuditMetadataKey.PID, s_ProcessId);
                audit.AddMetadata(AuditMetadataKey.ProcessName, s_ProcessName);
                audit.AddMetadata(AuditMetadataKey.SessionId, principal?.FindFirst(SanteDBClaimTypes.SanteDBSessionIdClaim)?.Value);
                audit.AddMetadata(AuditMetadataKey.CorrelationToken, rc?.CorrelationToken);
                audit.AddMetadata(AuditMetadataKey.AuditSourceType, "4");
                audit.AddMetadata(AuditMetadataKey.LocalEndpoint, rc?.OriginalRequestUrl);
                audit.AddMetadata(AuditMetadataKey.RemoteHost, rc?.RemoteAddress);
                audit.AddMetadata(AuditMetadataKey.ForwardInformation, rc?.ForwardInformation);
                audit.AddMetadata(AuditMetadataKey.EnterpriseSiteID, _Configuration?.SourceInformation?.EnterpriseSite);
                //audit.AddMetadata(AuditMetadataKey.AuditSourceID, (s_configuration?.SourceInformation?.EnterpriseDeviceKey ?? null)?.ToString());
            }
            catch (Exception e)
            {
                _Tracer.TraceError("Error preparing audit: {0}", e);
            }
        }

        ///<inheritdoc />
        public void DispatchAudit(AuditEventData audit)
        {
            using (AuthenticationContext.EnterSystemContext())
            {
                bool savelocal = false, dispatchremote = false;
                if (_Configuration?.ApplyFilters(audit, out savelocal, out dispatchremote) == true) //True indicates either savelocal or dispatchremote is true
                {
                    try
                    {
                        if (savelocal)
                        {
                            if (null == _AuditRepository)
                            {
                                _Tracer.TraceWarning("Audit configuration indicates audit should be dispatched to the local repository but no local repository is available.");
                            }
                            else
                            {
                                _Tracer.TraceVerbose("Inserting audit into local repository.");
                                _AuditRepository.Insert(audit);
                            }
                        }

                        if (dispatchremote)
                        {
                            if (null == _Dispatcher)
                            {
                                _Tracer.TraceWarning("Audit configuration indicates audit should be dispatched remotely but no service is configured.");
                            }
                            else
                            {
                                _Tracer.TraceVerbose("Dispatching audit to remote audit service.");
                                _Dispatcher.SendAudit(audit);
                            }
                        }
                    }
                    catch (PolicyViolationException polvex)
                    {
                        _Tracer.TraceError("Policy Violation Exception dispatching audit. Principal: {0}, Policy: {1}, Outcome {2}{3}{4}\r\nAudit: {5}", polvex.Principal?.Identity?.Name ?? "UNKNOWN", polvex.PolicyId, polvex.PolicyDecision.ToString(), Environment.NewLine, polvex.ToString(), audit.ToDisplay());
                        _Tracer.TraceEvent(System.Diagnostics.Tracing.EventLevel.Critical, "!!!!!!!!! CRITICAL !!!!!!! Policy Violation dispatching audit. This is a configuration issue and needs to be corrected to use SanteDB.");
                        Environment.Exit(911);
                    }
                }
            }
        }


        /// <summary>
        /// Private implementation of IAuditBuilder that will construct an audit to be sent via the service
        /// </summary>
        private class AuditServiceAuditEventData : IAuditBuilder
        {
            readonly DefaultAuditService _AuditService;

            public AuditServiceAuditEventData(DefaultAuditService auditService)
            {
                _AuditService = auditService;
                Audit = new AuditEventData();
            }

            public AuditServiceAuditEventData(DefaultAuditService auditService, DateTimeOffset timeStamp, ActionType actionCode, OutcomeIndicator outcome, EventIdentifierType eventIdentifier, AuditCode eventTypeCode)
            {
                _AuditService = auditService;
                Audit = new AuditEventData(timeStamp, actionCode, outcome, eventIdentifier, eventTypeCode);
            }

            [XmlIgnore, JsonIgnore]
            public AuditEventData Audit { get; }

            public void Send()
            {
                _AuditService.PrepareAuditForSend(Audit);
                _AuditService.SendAudit(Audit);
            }
        }



    }
}
