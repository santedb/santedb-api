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
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a notification service which manages the sending of a notification
    /// </summary>
    [System.ComponentModel.Description("User Notification Relay Provider")]
    public interface INotificationService : IServiceImplementation
    {

        /// <summary>
        /// Gets the notification relays available (SMS, EMAIL, etc.)
        /// </summary>
        IEnumerable<INotificationRelay> Relays { get; }

        /// <summary>
        /// Gets the specified notification relay
        /// </summary>
        /// <param name="toAddress">The address to retrieve the relay for.</param>
        INotificationRelay GetNotificationRelay(Uri toAddress);

        /// <summary>
        /// Gets the specified notification relay
        /// </summary>
        /// <param name="toAddress">The address to retrieve the relay for.</param>
        INotificationRelay GetNotificationRelay(String toAddress);

        /// <summary>
        /// Send a notification to one or more addresses. 
        /// </summary>
        /// <param name="to">The scheme qualified addresses to send the notification to.</param>
        /// <param name="subject">The subject for the notification. Some relays do not support a subject and will ignore it.</param>
        /// <param name="body">The body of the notification. All relays should support the body.</param>
        /// <param name="scheduleDelivery">Delay sending the notification to a particular time in the future.</param>
        /// <param name="ccAdmins">True to also send the notification to the defined administrators in the configuration.</param>
        /// <param name="attachments">Zero or more attachments to include with the notification.</param>
        /// <returns>An array of identifiers that correspond to the notifications.</returns>
        Guid[] SendNotification(string[] to, string subject, string body, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments);

        /// <summary>
        /// Send a notification using a template to one or more addresses.
        /// </summary>
        /// <param name="to">The scheme qualified addresses to send the notification to.</param>
        /// <param name="templateId">The template identifier that is available in the <see cref="INotificationTemplateRepository"/>.</param>
        /// <param name="templateLanguage">The language spoken to define which template language variant to use.</param>
        /// <param name="templateModel">The data model to use when filling the template.</param>
        /// <param name="scheduleDelivery">Delay sending the notification to a particular time in the future.</param>
        /// <param name="ccAdmins">True to also send the notification to the defined administrators in the configuration.</param>
        /// <param name="attachments">Zero or more attachments to include with the notification.</param>
        /// <returns>An array of identifiers that correspond to the notifications.</returns>
        Guid[] SendTemplatedNotification(string[] to, string templateId, string templateLanguage, dynamic templateModel, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments);
    }
}
