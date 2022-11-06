/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using SanteDB.Core.Queue;
using SanteDB.Core.Exceptions;
using System.Diagnostics;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// An implementation of <see cref="IDaemonService"/> which monitors instances of <see cref="IIdentityProviderService"/>
    /// and <see cref="ISessionIdentityProviderService"/> to audit login and logout events in the audit repository
    /// </summary>
    [ServiceProvider(SERVICE_NAME, required: true)]
    public class AuditDaemonService : IAuditDaemon
    {
        private const string SERVICE_NAME = "Security Audit Service";

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => SERVICE_NAME;

        private bool m_safeToStop = false;

        // Tracer class
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(AuditDaemonService));

        private IAuditService _AuditService;
        private IDispatcherQueueManagerService _QueueService;

        /// <summary>
        ///  True if the service is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// The service has started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// The service is starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// The service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// The service is stopping
        /// </summary>
        public event EventHandler Stopping;

        private void AuditIdentityProviderAuthenticated(object sender, AuthenticatedEventArgs args)
        {
            _AuditService.Audit().ForUserLogin(args.Principal, args.Success).Send();
        }

        private void AuditSessionProviderEstablished(object sender, SessionEstablishedEventArgs args)
        {
            if (args.Elevated)
            {
                _AuditService.Audit().ForOverride(args.Session, args.Principal, args.Purpose, args.Policies, args.Success).Send();
            }
            _AuditService.Audit().ForSessionStart(args.Session, args.Principal, args.Success).Send();
        }

        private void AuditSessionProviderAbandoned(object sender, SessionEstablishedEventArgs se)
        {
            _AuditService.Audit().ForSessionStop(se.Session, se.Principal, se.Success).Send();
        }

        private void AuditQueueMessageReceived(DispatcherMessageEnqueuedInfo e)
        {
            DispatcherQueueEntry entry = null;
            while (_QueueService.TryDequeue(AuditConstants.QueueName, out entry))
            {
                if (entry.Body is AuditEventData audit)
                {
                    try
                    {
                        _AuditService.DispatchAudit(audit);
                    }
                    catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
                    {
                        m_tracer.TraceError("Error dispatching audit - {0}", ex);
                        _QueueService.Enqueue(AuditConstants.DeadletterQueueName, audit);
                    }

                }
                else
                {
                    m_tracer.TraceWarning("Received message on audit queue but the type is not AuditEventData. Type is {0}.", entry.Body?.GetType()?.FullName);
                }
            }
        }

        private void StartListeningToServiceEvents()
        {
            var idsvc = ApplicationServiceContext.Current.GetService<IIdentityProviderService>();
            if (idsvc != null)
            {
                m_tracer.TraceVerbose($"Adding Event Listener for {nameof(IIdentityProviderService)}.{nameof(IIdentityProviderService.Authenticated)}");
                idsvc.Authenticated += AuditIdentityProviderAuthenticated;
            }

            var sessprov = ApplicationServiceContext.Current.GetService<ISessionProviderService>();

            if (sessprov != null)
            {
                m_tracer.TraceVerbose($"Adding Event Listener for {nameof(ISessionProviderService)}.{nameof(ISessionProviderService.Established)}");
                sessprov.Established += AuditSessionProviderEstablished;
                m_tracer.TraceVerbose($"Adding Event Listener for {nameof(ISessionProviderService)}.{nameof(ISessionProviderService.Abandoned)}");
                sessprov.Abandoned += AuditSessionProviderAbandoned;
            }
        }

        private void StopListeningToServiceEvents()
        {
            var idsvc = ApplicationServiceContext.Current.GetService<IIdentityProviderService>();
            if (idsvc != null)
            {
                idsvc.Authenticated -= AuditIdentityProviderAuthenticated;
            }

            var sessprov = ApplicationServiceContext.Current.GetService<ISessionProviderService>();

            if (sessprov != null)
            {
                sessprov.Established -= AuditSessionProviderEstablished;
                sessprov.Abandoned -= AuditSessionProviderAbandoned;
            }
        }

        private void StartAuditQueueSubscription()
        {
            _QueueService = ApplicationServiceContext.Current.GetService<IDispatcherQueueManagerService>();

            if (null == _QueueService)
            {
                m_tracer.TraceInfo("No {0} service is configured. No subscription will be configured.", nameof(IDispatcherQueueManagerService));
            }
            else
            {
                m_tracer.TraceVerbose("Opending audit queue {0} and deadletter queue {1}", AuditConstants.QueueName, AuditConstants.DeadletterQueueName);
                _QueueService.Open(AuditConstants.QueueName);
                _QueueService.Open(AuditConstants.DeadletterQueueName);

                m_tracer.TraceVerbose("Subscribing to audit queue {0}", AuditConstants.QueueName);
                _QueueService.SubscribeTo(AuditConstants.QueueName, AuditQueueMessageReceived);
            }
        }

        private void StopAuditQueueSubscription()
        {
            //QueueService can be null if no queue is configured. This will not matter in this operation.
            if (null != _QueueService)
            {
                m_tracer.TraceVerbose("Unsubscribing to audit queue {0}", AuditConstants.QueueName);
                _QueueService.UnSubscribe(AuditConstants.QueueName, AuditQueueMessageReceived);
            }
        }

        /// <summary>
        /// Start auditor service
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            this.m_safeToStop = false;
            ApplicationServiceContext.Current.Stopping += (o, e) =>
            {
                this.m_safeToStop = true;
                this.Stop();
            };
            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                try
                {
                    m_tracer.TraceVerbose("Getting instance of audit service for events.");
                    _AuditService = ApplicationServiceContext.Current.GetAuditService();

                    m_tracer.TraceInfo("Starting Audit Queue subscription.");
                    StartAuditQueueSubscription();

                    this.m_tracer.TraceInfo("Binding to service events.");

                    StartListeningToServiceEvents();

                    // Audit that Audits are now being recorded
                    _AuditService.Audit(DateTimeOffset.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, AuditUtil.CreateAuditActionCode(EventTypeCodes.AuditLoggingStarted))
                        .WithLocalSource()
                        .Send();
                }
                catch (Exception ex)
                {
                    this.m_tracer.TraceError("Error starting up audit daemon service: {0}", ex);
                }
            };

            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stopped
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            // Audit tool should never stop!!!!!
            if (!this.m_safeToStop)
            {
                _AuditService.Audit(DateTimeOffset.Now, ActionType.Execute, OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, AuditUtil.CreateAuditActionCode(EventTypeCodes.AuditLoggingStopped))
                    .WithLocalSource()
                    .Send();
            }
            else
            {
                m_tracer.TraceInfo("Unbinding to service events.");
                StopListeningToServiceEvents();

                m_tracer.TraceInfo("Stopping audit queue subscription.");
                StopAuditQueueSubscription();

                // Audit that audits are no longer being recorded
                _AuditService.Audit(DateTimeOffset.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, AuditUtil.CreateAuditActionCode(EventTypeCodes.AuditLoggingStopped))
                    .WithLocalSource()
                    .Send();
            };

            _AuditService = null;

            return true;
        }
    }
}