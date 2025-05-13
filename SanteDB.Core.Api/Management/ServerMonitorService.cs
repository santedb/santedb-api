/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
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
        private readonly INotifyDeviceIdentityProviderService m_notifyDeviceIdentityProviderService;


        /// <summary>
        /// DI constructor
        /// </summary>
        public ServerMonitorService(IConfigurationManager configurationManager, 
            INotificationService notificationService, 
            INetworkInformationService networkInformationService, 
            INotificationTemplateRepository notificationTemplateRepository,
            INotifyIdentityProviderService notifyIdentityProvider = null,
            INotifyDeviceIdentityProviderService notifiyDeviceIdentityProvider = null)
        {
            this.m_configuration = configurationManager.GetSection<ServerMonitorConfigurationSection>();
            this.m_notificationService = notificationService;
            this.m_notificationTemplateRepository = notificationTemplateRepository;
            this.m_networkInformationService = networkInformationService;
            this.m_notifyIdentityService = notifyIdentityProvider;
            this.m_notifyDeviceIdentityProviderService = notifiyDeviceIdentityProvider;
        }

        #region Notification Handlers 

        private void OnApplicationStarted(object sender, EventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.ServerStarted, sender, e);
        private void OnApplicationStopped(object sender, EventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.ServerStopped, sender, e);
        private void OnIdentityCreated(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserCreated, sender, e);
        private void OnIdentityDeleted(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserDeleted, sender, e);
        private void OnIdentityChanged(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserAlter, sender, e);
        private void OnIdentityClaimChanged(object sender, IdentityClaimEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserAlter, sender, e);
        private void OnDeviceUnlocked(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.DeviceUnlocked, sender, e);
        private void OnDeviceLocked(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.DeviceLocked, sender, e);
        private void OnIdentityUnLocked(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserUnlocked, sender, e);
        private void OnIdentityLocked(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.UserLocked, sender, e);
        private void OnDeviceDeleted(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.DeviceDeleted, sender, e);
        private void OnDeviceCreated(object sender, IdentityEventArgs e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.DeviceCreated, sender, e);
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
                        if (!String.IsNullOrEmpty(o.TemplateId) && this.m_notificationTemplateRepository.Get(o.TemplateId) != null)
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
        private IDictionary<string, object> ToDictionary(ServerMonitorEventSubscriptionEvent eventType, Object sender, EventArgs args)
        {
            IDictionary<String, Object> retVal = new Dictionary<string, object>
            {
                { "type", args.GetType().Name },
                { "event", eventType },
                { "sender", sender },
                { "sourceHost", this.m_networkInformationService.GetHostName() },
                { "sourceMachine", this.m_networkInformationService.GetMachineName() },
                { "summary", args.ToString() }
            };
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
            this.m_notifyDeviceIdentityProviderService.Created += this.OnDeviceCreated;
            this.m_notifyDeviceIdentityProviderService.Deleted += this.OnDeviceDeleted;
            this.m_notifyDeviceIdentityProviderService.Locked += this.OnIdentityLocked;
            this.m_notifyDeviceIdentityProviderService.Unlocked += this.OnIdentityUnLocked;
            this.m_notifyDeviceIdentityProviderService.Locked += this.OnDeviceLocked;
            this.m_notifyDeviceIdentityProviderService.Unlocked += this.OnDeviceUnlocked;
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
