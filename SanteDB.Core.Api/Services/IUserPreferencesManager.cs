using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

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
