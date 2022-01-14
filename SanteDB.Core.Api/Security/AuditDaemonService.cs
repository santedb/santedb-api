/*
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
using SanteDB.Core.Auditing;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// An implementation of <see cref="IDaemonService"/> which monitors instances of <see cref="IIdentityProviderService"/>
    /// and <see cref="ISessionIdentityProviderService"/> to audit login and logout events in the audit repository
    /// </summary>
    [ServiceProvider("Security Audit Service")]
    public class AuditDaemonService : IDaemonService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Detailed Persistence Layer Audit Subscription Service";

        private bool m_safeToStop = false;

        // Tracer class
        private Tracer m_tracer = Tracer.GetTracer(typeof(AuditDaemonService));

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
                    this.m_tracer.TraceInfo("Binding to service events...");

                    if (ApplicationServiceContext.Current.GetService<IIdentityProviderService>() != null)
                        ApplicationServiceContext.Current.GetService<IIdentityProviderService>().Authenticated += (so, se) =>
                        {
                            AuditUtil.AuditLogin(se.Principal, se.UserName, so as IIdentityProviderService, se.Success);
                        };
                    if (ApplicationServiceContext.Current.GetService<ISessionProviderService>() != null)
                    {
                        ApplicationServiceContext.Current.GetService<ISessionProviderService>().Established += (so, se) =>
                        {
                            if (se.Elevated)
                                AuditUtil.AuditOverride(se.Session, se.Principal, se.Purpose, se.Policies, se.Success);
                            AuditUtil.AuditSessionStart(se.Session, se.Principal, se.Success);
                        };
                        ApplicationServiceContext.Current.GetService<ISessionProviderService>().Abandoned += (so, se) => AuditUtil.AuditSessionStop(se.Session, se.Principal, se.Success);
                    }
                    // Audit that Audits are now being recorded
                    var audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, AuditUtil.CreateAuditActionCode(EventTypeCodes.AuditLoggingStarted));
                    AuditUtil.AddLocalDeviceActor(audit);
                    AuditUtil.SendAudit(audit);

                }
                catch (Exception ex)
                {
                    this.m_tracer.TraceError("Error starting up audit repository service: {0}", ex);
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
                AuditData securityAlertData = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, AuditUtil.CreateAuditActionCode(EventTypeCodes.AuditLoggingStopped));
                AuditUtil.AddLocalDeviceActor(securityAlertData);
                AuditUtil.SendAudit(securityAlertData);
            }
            else
            {
                // Audit that audits are no longer being recorded
                var audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, AuditUtil.CreateAuditActionCode(EventTypeCodes.AuditLoggingStopped));
                AuditUtil.AddLocalDeviceActor(audit);
                AuditUtil.SendAudit(audit);
            };

            return true;
        }
    }
}
