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

            if (null == template)
                throw new KeyNotFoundException($"Could not find template id: \"{templateId}\".");

            return FillTemplateInternal(template, templateLanguage, model);

            //var templateContent = template.Contents.FirstOrDefault(o => o.Language == templateLanguage || String.IsNullOrEmpty(o.Language));

                //if (null == template || null == templateContent)
                //{
                //    throw new KeyNotFoundException($"Could not find template id: \"{templateId}\" and language {templateLanguage}.");
                //}

                //string replacer(Match match)
                //{
                //    var nameGroup = match.Groups[1];

                //    if (nameGroup.Success != true || string.IsNullOrWhiteSpace(nameGroup.Value))
                //        return null;

                //    if (model.TryGetValue(nameGroup.Value, out var val))
                //        return val?.ToString();

                //    return null;
                //};

                //return new NotificationTemplateContents()
                //{
                //    Body = m_parmRegex.Replace(templateContent.Body, replacer),
                //    Subject = m_parmRegex.Replace(templateContent.Subject, replacer),
                //    Language = templateContent.Language
                //};
        }

        ///<inheritdoc />
        public NotificationTemplateContents FillTemplate(NotificationInstance template, string templateLanguage, IDictionary<string, object> model)
        {
            if (null == template)
                throw new ArgumentNullException(nameof(template), "Template is required.");

            return FillTemplateInternal(template.NotificationTemplate, templateLanguage, model);

            //var templateContent = template.NotificationTemplate.Contents.FirstOrDefault(o => o.Language == templateLanguage || String.IsNullOrEmpty(o.Language));

            //if (null == template || null == templateContent)
            //{
            //    throw new KeyNotFoundException($"Could not find template id: \"{template.NotificationTemplate.Key}\" and language {templateLanguage}.");
            //}

            //string replacer(Match match)
            //{
            //    var nameGroup = match.Groups[1];

            //    if (nameGroup.Success != true || string.IsNullOrWhiteSpace(nameGroup.Value))
            //        return null;

            //    if (model.TryGetValue(nameGroup.Value, out var val))
            //        return val?.ToString();
                
            //    return null;
            //};

            //return new NotificationTemplateContents()
            //{
            //    Body = m_parmRegex.Replace(templateContent.Body, replacer),
            //    Subject = m_parmRegex.Replace(templateContent.Subject, replacer),
            //    Language = templateContent.Language
            //};
        }

        /// <summary>
        /// Internal implementation resposible for both <see cref="FillTemplate(string, string, IDictionary{string, object})"/> and <see cref="FillTemplate(NotificationInstance, string, IDictionary{string, object})"/>.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="templateLanguage"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private NotificationTemplateContents FillTemplateInternal(NotificationTemplate template, string templateLanguage, IDictionary<string, object> model)
        {
            var content = template.Contents.FirstOrDefault(o => o.Language == templateLanguage || string.IsNullOrEmpty(o.Language));

            if (null == content)
            {
                throw new ArgumentException("Template does not contain default content and does not have matching language content.", nameof(template));
            }

            string replacer(Match match)
            {
                var nameGroup = match.Groups[1];

                if (nameGroup.Success != true || string.IsNullOrWhiteSpace(nameGroup.Value))
                    return null;

                if (model.TryGetValue(nameGroup.Value, out var val))
                    return val?.ToString();

                return null;
            }
            ;

            return new NotificationTemplateContents()
            {
                Body = m_parmRegex.Replace(content.Body, replacer),
                Subject = m_parmRegex.Replace(content.Subject, replacer),
                Language = content.Language
            };
        }
    }
}
