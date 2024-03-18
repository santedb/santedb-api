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
 * User: fyfej
 * Date: 2023-6-21
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Notifications.Templating
{
    /// <summary>
    /// A simple notification template filler that uses ${value} for inputs
    /// </summary>
    public class SimpleNotificationTemplateFiller : INotificationTemplateFiller
    {
        private readonly INotificationTemplateRepository m_notificationTemplateRepository;
        private static readonly Regex m_parmRegex = new Regex(@"\$\{([\w_][\-\d\w\._]*?)\}", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Get the service name
        /// </summary>
        public string ServiceName => "Simple Notification Template Filler";

        /// <summary>
        /// DI ctor
        /// </summary>
        public SimpleNotificationTemplateFiller(INotificationTemplateRepository notificationTemplateRepository)
        {
            this.m_notificationTemplateRepository = notificationTemplateRepository;
        }

        /// <summary>
        /// Fill the specified template based on identifier
        /// </summary>
        public NotificationTemplate FillTemplate(string id, string lang, dynamic model)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(model.GetType());
            var modelDict = model as IDictionary<String, Object>;
            var template = this.m_notificationTemplateRepository.Get(id, lang);
            if (template == null)
            {
                throw new KeyNotFoundException(id);
            }

            return new NotificationTemplate()
            {
                Id = template.Id,
                Body = m_parmRegex.Replace(template.Body, o => properties[o.Groups[1].Value]?.GetValue(model)?.ToString() ?? (modelDict.TryGetValue(o.Groups[1].Value, out var v) ? v?.ToString() : null)),
                Subject = m_parmRegex.Replace(template.Subject, o => properties[o.Groups[1].Value]?.GetValue(model)?.ToString() ?? (modelDict.TryGetValue(o.Groups[1].Value, out var v) ? v?.ToString() : null)),
                Language = template.Language
            };
        }
    }
}
