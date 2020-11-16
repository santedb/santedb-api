using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Notifications
{

    /// <summary>
    /// Represents a service which takes / provides structured templates into structured message objects
    /// </summary>
    public interface INotificationTemplateRepository : IServiceImplementation
    {

        /// <summary>
        /// Insert the specified template
        /// </summary>
        NotificationTemplate Insert(NotificationTemplate template);

        /// <summary>
        /// Updates the specified template
        /// </summary>
        NotificationTemplate Update(NotificationTemplate template);

        /// <summary>
        /// Gets the specified template
        /// </summary>
        NotificationTemplate Get(String id, String lang);

        /// <summary>
        /// Find the specified template
        /// </summary>
        IEnumerable<NotificationTemplate> Find(Expression<Func<NotificationTemplate, bool>> filter);
    }
}
