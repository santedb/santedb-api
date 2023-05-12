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
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A special type of configuration manager that allows users to set custom settings 
    /// (like views, color schemes, etc)
    /// </summary>
    public interface IUserPreferencesManager
    {

        /// <summary>
        /// Get the user settings for the <paramref name="forUser"/>
        /// </summary>
        /// <param name="forUser">The user identity to get the settings for</param>
        /// <returns>The dictionary of settings</returns>
        List<AppSettingKeyValuePair> GetUserSettings(String forUser);

        /// <summary>
        /// Set user settings for <paramref name="forUser"/>
        /// </summary>
        /// <param name="forUser">The user for which settings must be saved</param>
        /// <param name="settings">The settings to save in the manager</param>
        void SetUserSettings(String forUser, List<AppSettingKeyValuePair> settings);

    }
}
