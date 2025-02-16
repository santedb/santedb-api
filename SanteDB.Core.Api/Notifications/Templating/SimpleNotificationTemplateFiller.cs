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

        ///// <summary>
        ///// Fill the specified template based on identifier
        ///// </summary>
        //public NotificationTemplate FillTemplate(string id, string lang, dynamic model)
        //{
        //    if (model is IReadOnlyDictionary<string, object> dict)
        //        return FillTemplate(id, lang, dict);

        //    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(model.GetType());
        //    var modelDict = model as IDictionary<String, Object>;
        //    var template = this.m_notificationTemplateRepository.Get(id, lang);
        //    if (template == null)
        //    {
        //        throw new KeyNotFoundException(id);
        //    }

        //    string match_evaluator(Match o)
        //    {
        //        return properties[o.Groups[1].Value]?.GetValue(model)?.ToString() ?? (modelDict.TryGetValue(o.Groups[1].Value, out var v) ? v?.ToString() : null);
        //    }
        //    ;

        //    return new NotificationTemplate()
        //    {
        //        Id = template.Id,
        //        Body = m_parmRegex.Replace(template.Body, match_evaluator),
        //        Subject = m_parmRegex.Replace(template.Subject, match_evaluator),
        //        Language = template.Language
        //    };
        //}

        ///<inheritdoc />
        public NotificationTemplate FillTemplate(string templateId, string templateLanguage, IDictionary<string, object> model)
        {
            var template = m_notificationTemplateRepository.Get(templateId, templateLanguage);

            if (null == template)
            {
                throw new KeyNotFoundException($"Could not find template id: \"{templateId}\" and language {templateLanguage}.");
            }

            string replacer(Match match)
            {
                var namegroup = match.Groups[1];

                if (namegroup.Success != true || string.IsNullOrWhiteSpace(namegroup.Value))
                    return null;

                if (model.TryGetValue(namegroup.Value, out var val))
                    return val?.ToString() ?? null;
                else
                    return null;
            };

            return new NotificationTemplate()
            {
                Id = template.Id,
                Body = m_parmRegex.Replace(template.Body, replacer),
                Subject = m_parmRegex.Replace(template.Subject, replacer),
                Language = template.Language
            };
        }
    }
}
