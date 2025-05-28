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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Notifications.Templating
{
    /// <summary>
    /// A simple notification template filler that uses ${value} for inputs
    /// </summary>
    public class SimpleNotificationTemplateFiller : INotificationTemplateFiller
    {
        private readonly INotificationTemplateRepository m_notificationTemplateRepository;
        private static readonly Regex m_parmRegex = new Regex(@"\$\{([\w_][\-\d\w\._]*?)\}", RegexOptions.Multiline | RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

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

        ///<inheritdoc />
        public NotificationTemplateContents FillTemplate(string templateId, string templateLanguage, IDictionary<string, object> model)
        {
            var template = m_notificationTemplateRepository.Get(templateId);
            var templateContent = template.Contents.FirstOrDefault(o => o.Language == templateLanguage || String.IsNullOrEmpty(o.Language));

            if (null == template || null == templateContent)
            {
                throw new KeyNotFoundException($"Could not find template id: \"{templateId}\" and language {templateLanguage}.");
            }

            string replacer(Match match)
            {
                var nameGroup = match.Groups[1];

                if (nameGroup.Success != true || string.IsNullOrWhiteSpace(nameGroup.Value))
                    return null;

                if (model.TryGetValue(nameGroup.Value, out var val))
                    return val?.ToString();
                
                return null;
            };

            return new NotificationTemplateContents()
            {
                Body = m_parmRegex.Replace(templateContent.Body, replacer),
                Subject = m_parmRegex.Replace(templateContent.Subject, replacer),
                Language = templateContent.Language
            };
        }

        public NotificationTemplateContents FillTemplate(NotificationInstance template, string templateLanguage, IDictionary<string, object> model)
        {
            var templateContent = template.NotificationTemplate.Contents.FirstOrDefault(o => o.Language == templateLanguage || String.IsNullOrEmpty(o.Language));

            if (null == template || null == templateContent)
            {
                throw new KeyNotFoundException($"Could not find template id: \"{template.NotificationTemplate.Key}\" and language {templateLanguage}.");
            }

            string replacer(Match match)
            {
                var nameGroup = match.Groups[1];

                if (nameGroup.Success != true || string.IsNullOrWhiteSpace(nameGroup.Value))
                    return null;

                if (model.TryGetValue(nameGroup.Value, out var val))
                    return val?.ToString();
                
                return null;
            };

            return new NotificationTemplateContents()
            {
                Body = m_parmRegex.Replace(templateContent.Body, replacer),
                Subject = m_parmRegex.Replace(templateContent.Subject, replacer),
                Language = templateContent.Language
            };
        }
    }
}
