/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Configuration.Data
{

    
    
    /// <summary>
    /// Represents a storage provider
    /// </summary>
    public interface IDataConfigurationProvider : IReportProgressChanged
    {

        /// <summary>
        /// Gets the invariant name
        /// </summary>
        string Invariant { get; }

        /// <summary>
        /// Gets the name of the storage provider
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the platforms on which this storage provider works
        /// </summary>
        OperatingSystemID Platform { get; }

        /// <summary>
        /// Gets the hosts on which this storage provider works
        /// </summary>
        SanteDBHostType HostType { get; }

        /// <summary>
        /// Get the configuration options
        /// </summary>
        Dictionary<String, ConfigurationOptionType> Options { get; }

        /// <summary>
        /// Gets the groupings for the options
        /// </summary>
        Dictionary<String, String[]> OptionGroups { get; }

        /// <summary>
        /// Get the database provider type
        /// </summary>
        Type DbProviderType { get; }

        /// <summary>
        /// Creates the specified connection string
        /// </summary>
        ConnectionString CreateConnectionString(Dictionary<String, Object> options);

        /// <summary>
        /// Parse the specified connection string
        /// </summary>
        Dictionary<String, Object> ParseConnectionString(ConnectionString connectionString);

        /// <summary>
        /// Add the necessary information to the operating system configuration
        /// </summary>
        bool Configure(SanteDBConfiguration configuration, Dictionary<String, Object> options);

        /// <summary>
        /// Get data features matching this invariant name
        /// </summary>
        IEnumerable<IDataFeature> GetFeatures(ConnectionString connectionString);

        /// <summary>
        /// Get databases
        /// </summary>
        IEnumerable<String> GetDatabases(ConnectionString connectionString);

        /// <summary>
        /// Deploy the specified data feature to the specified configuration option
        /// </summary>
        bool Deploy(IDataFeature feature, String connectionStringName, SanteDBConfiguration configuration);

        /// <summary>
        /// Create the specified database in the provider
        /// </summary>
        ConnectionString CreateDatabase(ConnectionString connectionString, string databaseName, string databaseOwner);

        /// <summary>
        /// Tests the specified connection string
        /// </summary>
        bool TestConnectionString(ConnectionString connectionString);
    }
}


