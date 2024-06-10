using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a configuration section which discloses certain parameters down to a client dCDR
    /// </summary>
    public interface IDisclosedConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Instructs this configuration section to disclose public settings
        /// </summary>
        IEnumerable<AppSettingKeyValuePair> ForDisclosure();

        /// <summary>
        /// Injest settings from a disclosed kvp
        /// </summary>
        /// <param name="remoteSettings">The settings to be injested</param>
        void Injest(IEnumerable<AppSettingKeyValuePair> remoteSettings);

    }
}
