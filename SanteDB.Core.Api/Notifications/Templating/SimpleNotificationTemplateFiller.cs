using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Text;
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
            var modelDict = model as IDictionary<String, Object>;
            var template = this.m_notificationTemplateRepository.Get(id, lang);
            if(template == null)
            {
                throw new KeyNotFoundException(id);
            }

            return new NotificationTemplate()
            {
                Id = template.Id,
                Body = m_parmRegex.Replace(template.Body, o =>
                {
                    if (modelDict.TryGetValue(o.Groups[1].Value, out var value))
                    {
                        return value.ToString();
                    }
                    return "";
                }),
                Subject = m_parmRegex.Replace(template.Subject, o =>
                {
                    if (modelDict.TryGetValue(o.Groups[1].Value, out var value))
                    {
                        return value.ToString();
                    }
                    return "";
                }),
                Language = template.Language
            };
        }
    }
}
