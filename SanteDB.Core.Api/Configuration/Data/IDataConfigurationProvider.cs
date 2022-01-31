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
 * Date: 2021-8-27
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Configuration.Data
{
    /// <summary>
    /// Defines a database configuration provider
    /// </summary>
    /// <remarks>When running SanteDB iCDR or dCDR server, the configuration tooling uses implementations of these classes
    /// to fetch current databases, create new databases, reflect on connection properties, etc. Implementations of these classes
    /// should be tied directly to a particular plugin for a new database.</remarks>
    public interface IDataConfigurationProvider : IReportProgressChanged
    {
        /// <summary>
        /// Gets the invariant name of the database solution this configuration provider configures
        /// </summary>
        string Invariant { get; }

        /// <summary>
        /// Gets the name of the storage provider (for humans)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the platforms on which this storage provider works
        /// </summary>
        /// <remarks>
        /// SanteDB will use this information to restrict the selection of the data storage provider based on the
        /// environment in which the configured CDR is being executed.
        /// </remarks>
        OperatingSystemID Platform { get; }

        /// <summary>
        /// Gets the hosts on which this storage provider works
        /// </summary>
        /// <remarks>SanteDB will use this information to restrict the selection of data storage providers based on the
        /// type of dCDR or iCDR software the configuration is occurring within.</remarks>
        SanteDBHostType HostType { get; }

        /// <summary>
        /// Get the configuration options
        /// </summary>
        /// <remarks>The return value of this property represents all connection string options which should be exposed
        /// on the configuration screen which is using this instance. The resulting dictionary should have keys
        /// representing the property names in the connection string and the option types
        /// drive the drop down within the property grid.</remarks>
        IDictionary<String, ConfigurationOptionType> Options { get; }

        /// <summary>
        /// Gets the groupings for the options
        /// </summary>
        IDictionary<String, String[]> OptionGroups { get; }

        /// <summary>
        /// Get the database provider .NET type
        /// </summary>
        Type DbProviderType { get; }

        /// <summary>
        /// Creates the specified connection string with <paramref name="options"/>
        /// </summary>
        /// <param name="options">The options to be set on the connection string</param>
        /// <returns>The constructed <see cref="ConnectionString"/> instance</returns>
        ConnectionString CreateConnectionString(Dictionary<String, Object> options);

        /// <summary>
        /// Parse the specified connection string into a dictionary of key/value pairs
        /// </summary>
        /// <remarks>This method is used by the conifguration tooling to populate its property grid based on the current value of th e
        /// connection string in the configuration file being edited</remarks>
        /// <param name="connectionString">The connection string to parse</param>
        /// <returns>The values in the connection string as a key/value pair dictionary</returns>
        IDictionary<String, Object> ParseConnectionString(ConnectionString connectionString);

        /// <summary>
        /// Add the necessary information to the configuration file
        /// </summary>
        /// <remarks>This method is called once the user creates a new connection string with the provider, and the configuration subsystem
        /// wishes for the configuration procedure to append the necessary registrations and updates to the <paramref name="configuration"/></remarks>
        /// <param name="options">The options from the user interface which should be represented in the configuration file</param>
        /// <param name="configuration">The configuration file to which the configuration information should be appended.</param>
        bool Configure(SanteDBConfiguration configuration, IDictionary<String, Object> options);

        /// <summary>
        /// Get data features matching this invariant name
        /// </summary>
        /// <param name="connectionString">The connection string on which the provider should detect available updates</param>
        /// <returns>The features which can be applied to <paramref name="connectionString"/></returns>
        [Obsolete]
        IEnumerable<IDataFeature> GetFeatures(ConnectionString connectionString);

        /// <summary>
        /// Get all databases from the remote server
        /// </summary>
        /// <param name="connectionString">The connection string on which the databases should be retrieved (note: this may be a partial connection string)</param>
        /// <returns>The enumerator of all databases registered on the remote server in <paramref name="connectionString"/></returns>
        IEnumerable<String> GetDatabases(ConnectionString connectionString);

        /// <summary>
        /// Deploy the specified data feature to the specified configuration option
        /// </summary>
        /// <param name="configuration">The configuration to which the deployment information should be saved</param>
        /// <param name="connectionStringName">The name of the connection string on which the specified <paramref name="feature"/> should be deployed</param>
        /// <param name="feature">The feature which is to be deployed</param>
        /// <returns>True if the deployment succeeeded</returns>
        [Obsolete]
        bool Deploy(IDataFeature feature, String connectionStringName, SanteDBConfiguration configuration);

        /// <summary>
        /// Create the specified database in the provider
        /// </summary>
        /// <param name="connectionString">The connection string on which the database should be created (note: this may be a partial connection string)</param>
        /// <param name="databaseName">The name of the database which should be created</param>
        /// <param name="databaseOwner">The name of the user which owns the database</param>
        ConnectionString CreateDatabase(ConnectionString connectionString, string databaseName, string databaseOwner);

        /// <summary>
        /// Tests the specified connection string to ensure it is valid with the provider and the remote machine exists
        /// </summary>
        /// <param name="connectionString">The connection string which should be verified</param>
        /// <returns>True if the connection string is valid</returns>
        bool TestConnectionString(ConnectionString connectionString);
    }
}