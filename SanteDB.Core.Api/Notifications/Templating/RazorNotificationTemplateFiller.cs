﻿using RazorTemplates.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Notifications.Templating
{
    /// <summary>
    /// Represents a notification template filler that uses Razor engine
    /// </summary>
    public class RazorNotificationTemplateFiller : INotificationTemplateFiller
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Razor Notification Template Filler";

        // Templates
        private Dictionary<String, ITemplate<dynamic>> m_templates = new Dictionary<string, ITemplate<dynamic>>();

        /// <summary>
        /// Fill the template
        /// </summary>
        public NotificationTemplate FillTemplate(string id, string lang, dynamic model)
        {
            var repo = ApplicationServiceContext.Current.GetService<INotificationTemplateRepository>();
            if (repo == null)
                throw new InvalidOperationException("Cannot find notification template repository");

            var template = repo.Find(o => o.Id == id && o.Language == lang).FirstOrDefault();
            if (template == null)
                template = repo.Find(o => o.Id == id && o.Language == null).FirstOrDefault();

            if (template == null)
                throw new KeyNotFoundException($"Cannot find notification template {id}");

            // Now we want to set render
            if (!this.m_templates.TryGetValue($"{id}.{lang}.sub", out ITemplate<dynamic> subjectTemplate))
                this.m_templates.Add($"{id}.{lang}.sub", Template.Compile(template.Subject));
            if (!this.m_templates.TryGetValue($"{id}.{lang}.body", out ITemplate<dynamic> bodyTemplate))
                this.m_templates.Add($"{id}.{lang}.body", Template.Compile(template.Body));

            // Compose the return value
            try
            {
                return new NotificationTemplate()
                {
                    Id = id,
                    Language = lang,
                    Body = bodyTemplate.Render(model),
                    Subject = subjectTemplate.Render(model)
                };
            }
            catch(Exception e)
            {
                throw new Exception($"Error filling template {id}", e);
            }

        }
    }
}
