/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Event;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A user's preferences have been updated
    /// </summary>
    public class UserPreferencesUpdatedEventArgs : EventArgs
    {

        /// <summary>
        /// Create new updated user preferences
        /// </summary>
        public UserPreferencesUpdatedEventArgs(String userName, IEnumerable<AppSettingKeyValuePair> settings, Object settingsObject)
        {
            this.User = userName;
            this.Settings = settings;
            this.SettingsObject = settingsObject;
        }

        /// <summary>
        /// Gets the user which was updated
        /// </summary>
        public String User { get; }

        /// <summary>
        /// Gets the settings which were updated
        /// </summary>
        public IEnumerable<AppSettingKeyValuePair> Settings { get; }

        /// <summary>
        /// Gets the settings object that was actually updated
        /// </summary>
        public object SettingsObject { get; }
    }

    /// <summary>
    /// A special type of configuration manager that allows users to set custom settings 
    /// (like views, color schemes, etc)
    /// </summary>
    public interface IUserPreferencesManager
    {

        /// <summary>
        /// Event fired when user preferences are updated
        /// </summary>
        event EventHandler<UserPreferencesUpdatedEventArgs> Updated;

        /// <summary>
        /// Get the user settings for the <paramref name="forUser"/>
        /// </summary>
        /// <param name="forUser">The user identity to get the settings for</param>
        /// <returns>The dictionary of settings</returns>
        IEnumerable<AppSettingKeyValuePair> GetUserSettings(String forUser);

        /// <summary>
        /// Set user settings for <paramref name="forUser"/>
        /// </summary>
        /// <param name="forUser">The user for which settings must be saved</param>
        /// <param name="settings">The settings to save in the manager</param>
        void SetUserSettings(String forUser, IEnumerable<AppSettingKeyValuePair> settings);

    }
}
