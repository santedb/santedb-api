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
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Default list of feature groupings for the Configuration tooling
    /// </summary>
    public static class FeatureGroup
    {
        /// <summary>
        /// Feature is related to development
        /// </summary>
        public const string Development = "Development";

        /// <summary>
        /// Feature is related to diagnostics
        /// </summary>
        public const string Diagnostics = "Diagnostics";

        /// <summary>
        /// Feature is related to messaging
        /// </summary>
        public const string Messaging = "Messaging";

        /// <summary>
        /// Feature is an operating system / runtime feature
        /// </summary>
        public const string OperatingSystem = "Operating System";

        /// <summary>
        /// Performance
        /// </summary>
        public const string Performance = "Performance";

        /// <summary>
        /// Feature is related to persistence
        /// </summary>
        public const string Persistence = "Persistence";

        /// <summary>
        /// Feature is a security feature
        /// </summary>
        public const string Security = "Security";

        /// <summary>
        /// Feature is a system feature
        /// </summary>
        public const string System = "System";
    }

    /// <summary>
    /// Feature installation status
    /// </summary>
    public enum FeatureInstallState
    {
        /// <summary>
        /// The feature is fully installed
        /// </summary>
        Installed,

        /// <summary>
        /// The feature is partially installed - it may lack necessary information to start or operate in the current configuration context.
        /// </summary>
        PartiallyInstalled,

        /// <summary>
        /// The feature is not installed
        /// </summary>
        NotInstalled,

        /// <summary>
        /// The feature cannot be installed
        /// </summary>
        CantInstall
    }

    /// <summary>
    /// Identifies the flags for configuration
    /// </summary>
    [Flags]
    public enum FeatureFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The feature should always be configured
        /// </summary>
        AlwaysConfigure = 0x1,

        /// <summary>
        /// The task should be executed automatically if not already run
        /// </summary>
        AutoSetup = 0x2,

        /// <summary>
        /// The feature is a system feature and cannot be uninstalled.
        /// </summary>
        NoRemove = 0x4,

        /// <summary>
        /// The feature is a system feature
        /// </summary>
        SystemFeature = 0x8 | NoRemove | AutoSetup | AlwaysConfigure,

        /// <summary>
        /// Non-public feature
        /// </summary>
        NonPublic = 0x10
    }

    /// <summary>
    /// Configuration options type
    /// </summary>
    public enum ConfigurationOptionType
    {
        /// <summary>
        /// Option is a user token
        /// </summary>
        User,

        /// <summary>
        /// Option is a string
        /// </summary>
        String,

        /// <summary>
        /// Option is a boolean
        /// </summary>
        Boolean,

        /// <summary>
        /// Option is a numeric
        /// </summary>
        Numeric,

        /// <summary>
        /// Option is a password
        /// </summary>
        Password,

        /// <summary>
        /// Option is a filename
        /// </summary>
        FileName,

        /// <summary>
        /// Database name
        /// </summary>
        DatabaseName,

        /// <summary>
        /// Object type
        /// </summary>
        Object,

        /// <summary>
        /// Certificate picker for an X509 certificate  
        /// </summary>
        Certificate
    }

    /// <summary>
    /// Defines a feature in the context of the configuration tooling
    /// </summary>
    /// <remarks><para>The configuration tooling in SanteDB exposes a list of grouped "features" which can be configured using the SanteDB configuration tool. Each feature
    /// defines a configuration object which the configuration tooling uses to render a property grid (allowing users to change the values of the setting). When the configuration
    /// is applied, the feature is instructed to create either installation tasks or un-installation tasks</para>
    /// <para>Each <see cref="IConfigurationTask"/> created for install or un-install is responsible for changing the configuration file, the host operating system,
    /// or any other steps necessary to enable to remove the feature from the SanteDB instance.</para></remarks>
    public interface IFeature
    {
        /// <summary>
        /// Gets or sets the configuration object which the feature wishes the configuration tool to render.
        /// </summary>
        object Configuration { get; set; }

        /// <summary>
        /// Gets or sets the type of configuration section which this feature is expecting
        /// </summary>
        Type ConfigurationType { get; }

        /// <summary>
        /// Gets a human readable description of the feature which can be shown on the user interface
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the flags for this feature (such as auto-configure, always apply configuration, etc.)
        /// </summary>
        FeatureFlags Flags { get; }

        /// <summary>
        /// The group in which this feature belongs
        /// </summary>
        /// <seealso cref="FeatureGroup" />
        string Group { get; }

        /// <summary>
        /// Gets the name of the feature to show in the main configuration panel
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates one or more <see cref="IConfigurationTask"/> instances which can be executed to setup the feature
        /// </summary>
        /// <seealso cref="IConfigurationTask"/>
        IEnumerable<IConfigurationTask> CreateInstallTasks();

        /// <summary>
        /// Creates one or more <see cref="IConfigurationTask"/> instances which can be executed to remove the feature from SanteDB
        /// </summary>
        IEnumerable<IConfigurationTask> CreateUninstallTasks();

        /// <summary>
        /// Query the status of the feature in <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration which is being edited by the configuration tool. This is the configuration in which the feature implementation should look for install state</param>
        /// <returns>The feature installation state</returns>
        FeatureInstallState QueryState(SanteDBConfiguration configuration);
    }
}