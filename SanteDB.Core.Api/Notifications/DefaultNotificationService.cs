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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Default notification relay service that scans the current appdomain for relays
    /// </summary>
    public class DefaultNotificationService : INotificationService
    {
        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DefaultNotificationService));

        // Relay cache
        private IDictionary<String, INotificationRelay> m_relays;

        /// <summary>
        /// Get all relays
        /// </summary>
        public IEnumerable<INotificationRelay> Relays => this.m_relays.Values;

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Notification Relay Service";

        /// <summary>
        /// Default notification service
        /// </summary>
        public DefaultNotificationService()
        {
            this.m_relays = ApplicationServiceContext.Current.GetService<IServiceManager>()
                .GetAllTypes()
                .Where(t => typeof(INotificationRelay).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(t => Activator.CreateInstance(t) as INotificationRelay)
                .ToDictionary(o => o.Scheme, o => o);
        }

        /// <summary>
        /// Get the specified notification relay
        /// </summary>
        public INotificationRelay GetNotificationRelay(Uri toAddress)
        {
            if (this.m_relays.TryGetValue(toAddress.Scheme, out INotificationRelay retVal))
                return retVal;
            return null;
        }

        /// <summary>
        /// Get notification relay
        /// </summary>
        public INotificationRelay GetNotificationRelay(string toAddress) => this.GetNotificationRelay(new Uri(toAddress));

        /// <summary>
        /// Send the specified data to the specified addressers
        /// </summary>
        public Guid[] Send(string[] to, string subject, string body, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments)
        {

            var sendRelays = to.Select(o => new Uri(o)).GroupBy(o => o.Scheme);
            List<Guid> retVal = new List<Guid>(to.Length);
            foreach(var itm in sendRelays)
            {
                if (this.m_relays.TryGetValue(itm.Key, out INotificationRelay relay))
                {
                    retVal.Add(relay.Send(itm.Select(o => o.ToString()).ToArray(), subject, body, scheduleDelivery, ccAdmins, attachments));
                }
                else
                    this.m_tracer.TraceWarning("Cannot find relay on scheme {0}", itm.Key);
            }

            return retVal.ToArray();
        }
    }
}
