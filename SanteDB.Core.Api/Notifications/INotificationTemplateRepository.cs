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
 * Date: 2023-3-10
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Notifications
{

    /// <summary>
    /// Represents a service which takes / provides structured templates into structured message objects
    /// </summary>
    [System.ComponentModel.Description("User Notification Template Repository")]
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
