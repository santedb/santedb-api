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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Default notification relay service that scans the current appdomain for relays
    /// </summary>
    public class DefaultNotificationService : INotificationService
    {
        // Tracer
        private readonly Tracer m_tracer;

        private readonly INotificationTemplateRepository m_notificationTemplateRepository;
        private readonly INotificationTemplateFiller m_notificationTemplateFiller;

        // Relay cache
        private IDictionary<String, INotificationRelay> m_relays;

        /// <inheritdoc />
        public IEnumerable<INotificationRelay> Relays => this.m_relays.Values;

        /// <inheritdoc />
        public string ServiceName => "Notification Relay Service";

        /// <inheritdoc />
        public DefaultNotificationService(IServiceManager serviceManager, INotificationTemplateRepository templateRepository, INotificationTemplateFiller templateFiller)
        {
            this.m_tracer = new Tracer(nameof(DefaultNotificationService));
            this.m_relays = serviceManager
                .CreateInjectedOfAll<INotificationRelay>()
                .SelectMany(r=>r.SupportedSchemes.Select(s => (scheme: s, relay: r)))
                .ToDictionaryIgnoringDuplicates(o=>o.scheme, o=>o.relay);

            this.m_notificationTemplateRepository = templateRepository;
            this.m_notificationTemplateFiller = templateFiller;
        }

        /// <inheritdoc />
        public INotificationRelay GetNotificationRelay(Uri toAddress)
        {
            if (this.m_relays.TryGetValue(toAddress.Scheme, out INotificationRelay retVal))
            {
                return retVal;
            }

            return null;
        }

        /// <inheritdoc />
        public INotificationRelay GetNotificationRelay(string toAddress) => this.GetNotificationRelay(new Uri(toAddress));

        /// <inheritdoc />
        public Guid[] SendNotification(string[] to, string subject, string body, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments)
        {
            var sendRelays = to.Select(o => new Uri(o)).GroupBy(o => o.Scheme);
            List<Guid> retVal = new List<Guid>(to.Length);
            foreach (var itm in sendRelays)
            {
                if (this.m_relays.TryGetValue(itm.Key, out INotificationRelay relay))
                {
                    retVal.Add(relay.Send(itm.Select(o => o.ToString()).ToArray(), subject, body, scheduleDelivery, ccAdmins, attachments));
                }
                else
                {
                    this.m_tracer.TraceWarning("Cannot find relay on scheme {0}", itm.Key);
                }
            }

            return retVal.ToArray();
        }

        /// <inheritdoc />
        public Guid[] SendTemplatedNotification(string[] to, string templateId, string templateLanguage, dynamic templateModel, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments)
        {
            NotificationTemplate template = m_notificationTemplateFiller.FillTemplate(templateId, templateLanguage, templateModel);

            return SendNotification(to, template.Subject, template.Body, scheduleDelivery, ccAdmins, attachments);
        }
    }
}