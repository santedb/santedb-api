/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Services;
using System;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a service that can fill the template
    /// </summary>
    [System.ComponentModel.Description("User Notification Template Filler")]
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
