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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a service that can fill the template
    /// </summary>
    [System.ComponentModel.Description("User Notification Template Filler")]
    public interface INotificationTemplateFiller : IServiceImplementation
    {
        /// <summary>
        /// Retrieves a template from the repository using <paramref name="templateId"/> and <paramref name="templateLanguage"/> and fills it with key/value pairs provided in <paramref name="model"/>.
        /// </summary>
        /// <param name="templateId">The id of the template in the repository.</param>
        /// <param name="templateLanguage">The language of the template from the repository. The language should match the language preference of the entity which will receive the notification.</param>
        /// <param name="model">A key/value pair dictionary of values to use for insertion into the notification.</param>
        /// <returns>An instance of <see cref="NotificationTemplate"/> which has been filled in using the <paramref name="model"/> provided.</returns>
        NotificationTemplateContents FillTemplate(string templateId, string templateLanguage, IDictionary<string, object> model);

    }
}
