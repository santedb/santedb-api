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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Object flags which can be notified
    /// </summary>
    [Flags]
    public enum ServerNotificationObjects
    {
        /// <summary>
        /// Server application service
        /// </summary>
        ServerService = 0x100,
        /// <summary>
        /// Security users
        /// </summary>
        User = 0x200,
        /// <summary>
        /// Security Devices
        /// </summary>
        Device = 0x400,
        /// <summary>
        /// Security Applications
        /// </summary>
        Application = 0x800,
        /// <summary>
        /// Security Jobs
        /// </summary>
        Job = 0x1000,
        /// <summary>
        /// Server Configuration
        /// </summary>
        Configuration = 0x2000
    }

    /// <summary>
    /// Server notification events
    /// </summary>
    [Flags]
    public enum ServerNotificationEvents
    {
        /// <summary>
        /// Object is locked
        /// </summary>
        Locked = 0x1,
        /// <summary>
        /// Object is unlocked
        /// </summary>
        Unlocked = 0x2,
        /// <summary>
        /// Object has started
        /// </summary>
        Started = 0x4,
        /// <summary>
        /// Object has stopped
        /// </summary>
        Stopped = 0x8,
        /// <summary>
        /// Object encountered an error
        /// </summary>
        Error = 0x10,
        /// <summary>
        /// Object was created
        /// </summary>
        Created = 0x20,
        /// <summary>
        /// Object was deleted
        /// </summary>
        Deleted = 0x40,
        /// <summary>
        /// Object was altered
        /// </summary>
        Altered = 0x80
    }

    /// <summary>
    /// Service events which can be subscribed to
    /// </summary>
    [Flags]
    [XmlType(nameof(ServerNotificationEvents), Namespace = "http://santedb.org/configuration")]
    public enum ServerMonitorEventSubscriptionEvent
    {
        /// <summary>
        /// Server service has started
        /// </summary>
        [XmlEnum("server-start")]
        ServerStarted = ServerNotificationObjects.ServerService | ServerNotificationEvents.Started,
        /// <summary>
        /// Server service has stopped
        /// </summary>
        [XmlEnum("server-stop")]
        ServerStopped = ServerNotificationObjects.ServerService | ServerNotificationEvents.Stopped,
        /// <summary>
        /// User has been created
        /// </summary>
        [XmlEnum("user-create")]
        UserCreated = ServerNotificationObjects.User | ServerNotificationEvents.Created,
        /// <summary>
        /// User has been locked
        /// </summary>
        [XmlEnum("user-lock")]
        UserLocked = ServerNotificationObjects.User | ServerNotificationEvents.Locked,
        /// <summary>
        /// User has been unlocked
        /// </summary>
        [XmlEnum("user-unlock")]
        UserUnlocked = ServerNotificationObjects.User | ServerNotificationEvents.Unlocked,
        /// <summary>
        /// User has been deleted
        /// </summary>
        [XmlEnum("user-delete")]
        UserDeleted = ServerNotificationObjects.User | ServerNotificationEvents.Deleted,
        /// <summary>
        /// User has been altered
        /// </summary>
        [XmlEnum("user-alter")]
        UserAlter = ServerNotificationObjects.User | ServerNotificationEvents.Altered,
        /// <summary>
        /// Device has been created
        /// </summary>
        [XmlEnum("device-create")]
        DeviceCreated = ServerNotificationObjects.Device | ServerNotificationEvents.Created,
        /// <summary>
        /// Device has been locked
        /// </summary>
        [XmlEnum("device-lock")]
        DeviceLocked = ServerNotificationObjects.Device | ServerNotificationEvents.Locked,
        /// <summary>
        /// Device has been unlocked
        /// </summary>
        [XmlEnum("device-unlock")]
        DeviceUnlocked = ServerNotificationObjects.Device | ServerNotificationEvents.Unlocked,
        /// <summary>
        /// Device has been deleted
        /// </summary>
        [XmlEnum("device-delete")]
        DeviceDeleted = ServerNotificationObjects.Device | ServerNotificationEvents.Deleted,
        /// <summary>
        /// Application has been created
        /// </summary>
        [XmlEnum("app-create")]
        ApplicationCreated = ServerNotificationObjects.Application | ServerNotificationEvents.Created,
        /// <summary>
        /// Application has been locked
        /// </summary>
        [XmlEnum("app-lock")]
        ApplicationLocked = ServerNotificationObjects.Application | ServerNotificationEvents.Locked,
        /// <summary>
        /// Application has been unlocked
        /// </summary>
        [XmlEnum("app-unlock")]
        ApplicationUnlocked = ServerNotificationObjects.Application | ServerNotificationEvents.Unlocked,
        /// <summary>
        /// Application has been deleted
        /// </summary>
        [XmlEnum("app-delete")]
        ApplicationDeleted = ServerNotificationObjects.Application | ServerNotificationEvents.Deleted,
        /// <summary>
        /// Job has been started
        /// </summary>
        [XmlEnum("job-start")]
        JobStarted = ServerNotificationObjects.Job | ServerNotificationEvents.Started,
        /// <summary>
        /// Job has completed
        /// </summary>
        [XmlEnum("job-stop")]
        JobStopped = ServerNotificationObjects.Job | ServerNotificationEvents.Stopped,
        /// <summary>
        /// Job schedule has been altered
        /// </summary>
        [XmlEnum("job-schedule-alter")]
        JobAltered = ServerNotificationObjects.Job | ServerNotificationEvents.Altered,
        /// <summary>
        /// Job encountered an error
        /// </summary>
        [XmlEnum("job-error")]
        JobError = ServerNotificationObjects.Job | ServerNotificationEvents.Error,
        /// <summary>
        /// Configuration has been altered
        /// </summary>
        [XmlEnum("config-alter")]
        ConfigurationAltered = ServerNotificationObjects.Configuration | ServerNotificationEvents.Altered
    }

    /// <summary>
    /// Server monitoring configuration section 
    /// </summary>
    [XmlType(nameof(ServerMonitorConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class ServerMonitorConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Gets or sets the notification events
        /// </summary>
        [XmlArray("events"), XmlArrayItem("add"), JsonProperty("events")]
        public List<ServerMonitorEventSubscription> EventSubscriptions { get; set; }

    }

    /// <summary>
    /// Notification subscriptions
    /// </summary>
    [XmlType(nameof(ServerMonitorEventSubscription), Namespace = "http://santedb.org/configuration")]
    public class ServerMonitorEventSubscription
    {

        /// <summary>
        /// Gets or sets the events which trigger this subscription
        /// </summary>
        [XmlAttribute("when"), JsonProperty("when")]
        public ServerMonitorEventSubscriptionEvent When { get; set; }

        /// <summary>
        /// Gets or sets the notification targets (e-mail addresses)
        /// </summary>
        [XmlElement("notify"), JsonProperty("notify")]
        public List<String> Notify { get; set; }

        /// <summary>
        /// Template identifier to send
        /// </summary>
        [XmlAttribute("template"), JsonProperty("template")]
        public String TemplateId { get; set; }

    }
}
