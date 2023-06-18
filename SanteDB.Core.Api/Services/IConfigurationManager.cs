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
 * Date: 2023-5-19
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using System;
using System.ComponentModel;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Contract for service implementations that manage the core SanteDB configuration
    /// </summary>
    /// <remarks>
    /// <para>
    /// SanteDB plugins are expected to be portable and can run on a variety of platforms, in a variety of deployments, and a variety 
    /// of environments. This necessitates a consistent manner to manage configuration data for the SanteDB services. The <see cref="IConfigurationManager"/>
    /// is responsible for this duty. Example implementations of this service may include:
    /// </para>
    /// <list type="bullet">
    ///     <item>Loading configuration from a file stored on a local file system</item>
    ///     <item>Loading configuration from a shared document-based database (for distributed configurations)</item>
    ///     <item>Loading configuration from environment variables or synthesization classes (like in Docker)</item>
    ///     <item>Loading/chaining configuration from another or central iCDR instance</item>
    /// </list>
    /// <para>
    /// By default, the SanteDB iCDR and dCDR will use an XML or JSON file to store the configuration information, however the <see cref="SanteDBConfiguration"/>
    /// class can be shared on any number of transports.
    /// </para>
    /// </remarks>
    [Description("Configuration Manager Service")]
    public interface IConfigurationManager : IServiceImplementation
    {

        /// <summary>
        /// True if the configuration manager is readonly
        /// </summary>
        bool IsReadonly { get; }

        /// <summary>
        /// Get the specified configuration section
        /// </summary>
        /// <typeparam name="T">The configuration section type to retrieve</typeparam>
        /// <returns>The configuration section</returns>
        T GetSection<T>() where T : IConfigurationSection;

        /// <summary>
        /// Gets the specified application setting
        /// </summary>
        /// <remarks>App settings are intended to provide implementers with an easy 
        /// way to store simple settings without requiring the implementation of a 
        /// <see cref="IConfigurationSection"/> class</remarks>
        /// <param name="key">The key of the setting to retrieve</param>
        /// <returns>The setting for <paramref name="key"/> or null if none</returns>
        String GetAppSetting(String key);

        /// <summary>
        /// Get the specified connection string to a database
        /// </summary>
        /// <param name="key">The identifier of the connection string to retrieve</param>
        /// <returns>The database connection string</returns>
        ConnectionString GetConnectionString(String key);

        /// <summary>
        /// Get the entirety of the SanteDB configuration
        /// </summary>
        SanteDBConfiguration Configuration { get; }

        /// <summary>
        /// Set the specified application setting
        /// </summary>
        /// <param name="key">The key of the application setting to set</param>
        /// <param name="value">The value of the application setting</param>
        /// <remarks>Some configuration environments may not support the setting or modification of configuration programmatically. This may be
        /// the case in highly secured environments (like hospitals, national iCDR deployments). In that case, implementers should throw
        /// a <see cref="NotSupportedException"/></remarks>
        /// <exception cref="NotSupportedException">If the implementer does not permit setting of app settings prograatically</exception>
        void SetAppSetting(string key, string value);

        /// <summary>
        /// Forces the configuration manager to reload the current configuration
        /// </summary>
        void Reload();

        /// <summary>
        /// Save the configuration
        /// </summary>
        void SaveConfiguration();

        /// <summary>
        /// Adds a connection string only for the lifetime of the server
        /// </summary>
        /// <param name="name">The name of the connection string</param>
        /// <param name="connectionString">The connection string</param>
        void SetTransientConnectionString(string name, ConnectionString connectionString);
    }
}
