/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Principal;

namespace SanteDB.Core.Management
{
    /// <summary>
    /// Represents a security monitoring service which notifies the administrator based on a series of events
    /// </summary>
    public class ServerMonitorService : IDaemonService
    {
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ServerMonitorService));
        private const string NOTIFICATION_TEMPLATE_ID = "org.santedb.notifications.server.monitor";
        private readonly ServerMonitorConfigurationSection m_configuration;
        private readonly INotificationService m_notificationService;
        private readonly INotificationTemplateRepository m_notificationTemplateRepository;
        private readonly INetworkInformationService m_networkInformationService;
        private readonly INotifyIdentityProviderService m_notifyIdentityService;


        /// <summary>
        /// DI constructor
        /// </summary>
        public ServerMonitorService(IConfigurationManager configurationManager, 
            INotificationService notificationService, 
            INetworkInformationService networkInformationService, 
            INotificationTemplateRepository notificationTemplateRepository,
            INotifyIdentityProviderService notifyIdentityProvider = null)
        {
            this.m_configuration = configurationManager.GetSection<ServerMonitorConfigurationSection>();
            this.m_notificationService = notificationService;
            this.m_notificationTemplateRepository = notificationTemplateRepository;
            this.m_networkInformationService = networkInformationService;
            this.m_notifyIdentityService = notifyIdentityProvider;
        }

        #region Notification Handlers 

        private void OnApplicationStarted(object sender, EventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.ServerStarted, sender, e);
        private void OnApplicationStopped(object sender, EventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.ServerStopped, sender, e);
        private void OnIdentityCreated(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserCreated, sender, e);
        private void OnIdentityDeleted(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserDeleted, sender, e);
        private void OnIdentityChanged(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserAlter, sender, e);
        private void OnIdentityClaimChanged(object sender, IdentityClaimEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserAlter, sender, e);
        #endregion 

        /// <summary>
        /// True if the service is running
        /// </summary>
        public bool IsRunning => true;

        /// <summary>
        /// Get the service name
        /// </summary>
        public string ServiceName => "Service Monitoring Alert";

        /// <summary>
        /// Service is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Service has started
        /// </summary>
#pragma warning disable CS0067 // The event 'ServerMonitorService.Started' is never used
        public event EventHandler Started;
#pragma warning restore CS0067 // The event 'ServerMonitorService.Started' is never used
        /// <summary>
        /// Service is stopping
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Service has stopped
        /// </summary>
        public event EventHandler Stopped;


        /// <summary>
        /// Notify the event has occurred
        /// </summary>
        private void NotifyEvent(ServerMonitorEventSubscriptionEvent eventType, Object sender, EventArgs args)
        {

            this.m_configuration.EventSubscriptions.Where(o => o.When.HasFlag(eventType))
                .ForEach(o =>
                {
                    try
                    {
                        // Is there a special template for this notification
                        if (!String.IsNullOrEmpty(o.TemplateId) && this.m_notificationTemplateRepository.Get(o.TemplateId, String.Empty) != null)
                        {
                            this.m_notificationService.SendTemplatedNotification(o.Notify.ToArray(), o.TemplateId, String.Empty, this.ToDictionary(eventType, sender, args));
                        }
                        else
                        {
                            this.m_notificationService.SendTemplatedNotification(o.Notify.ToArray(), NOTIFICATION_TEMPLATE_ID, String.Empty, this.ToDictionary(eventType, sender, args)); // Send a generic template
                        }
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceWarning("Error sending server monitoring notification: {0} - {1}", eventType, e.ToHumanReadableString());
                    }
                });

        }

        /// <summary>
        /// Convert to a dictionary
        /// </summary>
        private dynamic ToDictionary(ServerMonitorEventSubscriptionEvent eventType, Object sender, EventArgs args)
        {
            IDictionary<String, Object> retVal = new ExpandoObject();
            retVal.Add("type", args.GetType().Name);
            retVal.Add("event", eventType);
            retVal.Add("sender", sender);
            retVal.Add("sourceHost", this.m_networkInformationService.GetHostName());
            retVal.Add("sourceMachine", this.m_networkInformationService.GetMachineName());
            retVal.Add("summary", args.ToString());
            foreach (var p in args.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var value = p.GetValue(args);
                switch(value)
                {
                    case IIdentity id:
                        retVal.Add(p.Name, id.Name);
                        break;
                    case IClaim clm:
                        retVal.Add(p.Name, $"claim:{clm.Type}={clm.Value}");
                        break;
                    default:
                        retVal.Add(p.Name, value);
                        break;
                }
               
            }
            return retVal;
        }

        /// <summary>
        /// Start the service
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started += this.OnApplicationStarted;
            ApplicationServiceContext.Current.Stopping += this.OnApplicationStopped;
            this.m_notifyIdentityService.Changed += this.OnIdentityChanged;
            this.m_notifyIdentityService.ClaimAdded += this.OnIdentityClaimChanged;
            this.m_notifyIdentityService.ClaimRemoved += this.OnIdentityClaimChanged;
            this.m_notifyIdentityService.Created += this.OnIdentityCreated;
            this.m_notifyIdentityService.Deleted += this.OnIdentityDeleted;

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stop this service
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started -= this.OnApplicationStarted;
            ApplicationServiceContext.Current.Stopping -= this.OnApplicationStopped;

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
