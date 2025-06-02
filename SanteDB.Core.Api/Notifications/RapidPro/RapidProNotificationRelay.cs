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
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SanteDB.Core.Notifications.RapidPro
{
    /// <summary>
    /// Notification relay that sends notifications through RapidPro
    /// </summary>
    public class RapidProNotificationRelay : INotificationRelay
    {
        readonly Tracer m_traceSource;
        readonly RapidProNotificationConfigurationSection m_configuration;

        private static readonly HttpClient m_httpClient = new HttpClient();

        /// <summary>
        /// DI constructor
        /// </summary>
        public RapidProNotificationRelay(IConfigurationManager configurationManager)
        {
            m_traceSource = Tracer.GetTracer(typeof(RapidProNotificationRelay));
            m_configuration = configurationManager.GetSection<RapidProNotificationConfigurationSection>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> SupportedSchemes => new[] { "rapidPro" };

        /// <summary>
        /// Send the specified notification through RapidPro
        /// </summary>
        public Guid Send(string[] toAddress, string subject, string body, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments)
        {
            try
            {
                m_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", this.m_configuration.ApiKey);
                m_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(this.m_configuration.UserAgent);
                m_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = m_httpClient.PostAsync($"{this.m_configuration.BaseAddress}/messages.json", new StringContent(body)).GetAwaiter().GetResult();

                response.EnsureSuccessStatusCode();

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                this.m_traceSource.TraceError("Error sending notification: {0}", ex);
                throw;
            }
        }
    }
}