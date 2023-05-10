﻿using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Notifications;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

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

        // Event handlers
        private readonly EventHandler APPLICATION_STARTED_HANDLER;
        private readonly EventHandler APPLICATION_STOPPED_HANDLER;

        /// <summary>
        /// DI constructor
        /// </summary>
        public ServerMonitorService(IConfigurationManager configurationManager, INotificationService notificationService, INetworkInformationService networkInformationService, INotificationTemplateRepository notificationTemplateRepository)
        {
            this.m_configuration = configurationManager.GetSection<ServerMonitorConfigurationSection>();
            this.m_notificationService = notificationService;
            this.m_notificationTemplateRepository = notificationTemplateRepository;
            this.m_networkInformationService = networkInformationService;
            this.APPLICATION_STARTED_HANDLER = (o, e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.ServerStarted, o, e);
            this.APPLICATION_STOPPED_HANDLER = (o, e) => this.NotifyEvent(ServerMonitorEventSubscriptionEvent.ServerStopped, o, e);
        }

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
        public event EventHandler Started;
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
                        if (!String.IsNullOrEmpty(o.TemplateId) && this.m_notificationTemplateRepository.Get(o.TemplateId, "en") != null)
                        {
                            this.m_notificationService.SendTemplatedNotification(o.Notify.ToArray(), o.TemplateId, "en", this.ToDictionary(eventType, sender, args));
                        }
                        else
                        {
                            this.m_notificationService.SendTemplatedNotification(o.Notify.ToArray(), NOTIFICATION_TEMPLATE_ID, "en", this.ToDictionary(eventType, sender, args)); // Send a generic template
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
        private dynamic  ToDictionary(ServerMonitorEventSubscriptionEvent eventType, Object sender, EventArgs args)
        {
            IDictionary<String, Object> retVal = new ExpandoObject();
            retVal.Add("type", args.GetType().Name);
            retVal.Add("event", eventType);
            retVal.Add("sender", sender);
            retVal.Add("sourceHost", this.m_networkInformationService.GetHostName());
            retVal.Add("sourceMachine", this.m_networkInformationService.GetMachineName());
            retVal.Add("summary", args.ToString());
            foreach(var p in args.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var value = p.GetValue(args);
                if(value != null)
                {
                    retVal.Add(p.Name, value);
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

            ApplicationServiceContext.Current.Started += APPLICATION_STARTED_HANDLER;
            ApplicationServiceContext.Current.Stopping += APPLICATION_STOPPED_HANDLER;

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stop this service
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started -= APPLICATION_STARTED_HANDLER;
            ApplicationServiceContext.Current.Stopping -= APPLICATION_STOPPED_HANDLER;

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}