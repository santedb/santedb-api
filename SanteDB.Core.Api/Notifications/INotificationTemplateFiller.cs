using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a service that can fill the template
    /// </summary>
    public interface INotificationTemplateFiller : IServiceImplementation
    {

        /// <summary>
        /// Fill the template
        /// </summary>
        /// <param name="id">The id of the template to be filled</param>
        /// <param name="lang">The language to fill</param>
        /// <param name="model">The model to use to fill</param>
        /// <returns>The filled template</returns>
        NotificationTemplate FillTemplate(String id, String lang, dynamic model);
    }
}
