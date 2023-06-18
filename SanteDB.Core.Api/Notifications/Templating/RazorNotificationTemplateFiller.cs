/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using RazorLight;
using RazorLight.Razor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanteDB.Core.Notifications.Templating
{
    /// <summary>
    /// Represents a notification template filler that uses Razor engine
    /// </summary>
    public class RazorNotificationTemplateFiller : INotificationTemplateFiller
    {

        /// <summary>
        /// Razor light project item
        /// </summary>
        private class SanteDBRazorProjectItem : RazorLightProjectItem
        {
            // Notification template
            private readonly NotificationTemplate m_template;

            // If true use the header 
            private readonly bool m_forSubject;

            /// <summary>
            /// Create a new razor project item
            /// </summary>
            public SanteDBRazorProjectItem(NotificationTemplate notificationTemplate, bool forHeader)
            {
                this.m_template = notificationTemplate;
                this.m_forSubject = forHeader;
            }

            /// <summary>
            /// Get the key of the template
            /// </summary>
            public override string Key => this.m_template.Id;

            /// <summary>
            /// Exists
            /// </summary>
            public override bool Exists => true;

            /// <summary>
            /// Read the content
            /// </summary>
            public override Stream Read() => new MemoryStream(Encoding.UTF8.GetBytes(this.m_forSubject ? this.m_template.Subject : this.m_template.Body));
        }

        /// <summary>
        /// Razor light project which uses the SanteDB template repository
        /// </summary>
        private class SanteDBRazorProject : RazorLightProject
        {

            /// <summary>
            /// Name regex
            /// </summary>
            private readonly Regex m_nameRegex = new Regex("^(.*?)-(.*?)-(.*?)$");

            // Repository
            private readonly INotificationTemplateRepository m_repository;

            /// <summary>
            /// Creates new project with the specified template repository
            /// </summary>
            public SanteDBRazorProject(INotificationTemplateRepository notificationTemplateRepository)
            {
                this.m_repository = notificationTemplateRepository;
            }

            /// <summary>
            /// Get imports
            /// </summary>
            public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
            {
                return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
            }


#pragma warning disable CS1998
            /// <summary>
            /// Get template item
            /// </summary>
            public override async Task<RazorLightProjectItem> GetItemAsync(string templateKey)
            {
                var templateMatch = this.m_nameRegex.Match(templateKey);
                if (templateMatch.Success)
                {
                    String templateId = templateMatch.Groups[1].Value,
                        languageId = templateMatch.Groups[2].Value,
                        parameterType = templateMatch.Groups[3].Value;
                    return new SanteDBRazorProjectItem(this.m_repository.Find(o => o.Id == templateId && o.Language == languageId).FirstOrDefault(), parameterType == "subject");
                }
                else
                {
                    throw new KeyNotFoundException(templateKey);
                }
            }
        }
#pragma warning restore
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Razor Notification Template Filler";

        // Razor engine
        private readonly RazorLightEngine m_razorEngine;

        // Compiled template pages
        private readonly IDictionary<String, ITemplatePage> m_compiledRazorPages = new ConcurrentDictionary<String, ITemplatePage>();

        /// <summary>
        /// Razor engine
        /// </summary>
        public RazorNotificationTemplateFiller(INotificationTemplateRepository notificationTemplateRepository)
        {
            this.m_razorEngine = new RazorLightEngineBuilder()
              .UseProject(new SanteDBRazorProject(notificationTemplateRepository))
              .UseMemoryCachingProvider()
              .Build();
        }
        /// <summary>
        /// Fill the template
        /// </summary>
        public NotificationTemplate FillTemplate(string id, string lang, dynamic model)
        {

            // Now we want to set render
            if (!this.m_compiledRazorPages.TryGetValue($"{id}.{lang}.sub", out var subjectTemplate))
            {
                this.m_compiledRazorPages.Add($"{id}.{lang}.sub", this.m_razorEngine.CompileTemplateAsync($"{id}-{lang}-subject").Result);
            }

            if (!this.m_compiledRazorPages.TryGetValue($"{id}.{lang}.body", out var bodyTemplate))
            {
                this.m_compiledRazorPages.Add($"{id}.{lang}.body", this.m_razorEngine.CompileTemplateAsync($"{id}-{lang}-body").Result);
            }

            // Compose the return value
            try
            {
                return new NotificationTemplate()
                {
                    Id = id,
                    Language = lang,
                    Body = this.m_razorEngine.RenderTemplateAsync(bodyTemplate, model, model.GetType(), model as ExpandoObject).Result,
                    Subject = this.m_razorEngine.RenderTemplateAsync(subjectTemplate, model, model.GetType(), model as ExpandoObject).Result
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Error filling template {id}", e);
            }

        }
    }
}
