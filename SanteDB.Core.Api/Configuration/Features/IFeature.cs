﻿/*
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
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Feature group constants
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
    /// Feature installation state
    /// </summary>
    public enum FeatureInstallState
    {
        /// <summary>
        /// The feature is fully installed
        /// </summary>
        Installed,
        /// <summary>
        /// The feature is partially installed
        /// </summary>
        PartiallyInstalled,
        /// <summary>
        /// The feature is not installed
        /// </summary>
        NotInstalled
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
        SystemFeature = 0x8 | NoRemove | AutoSetup | AlwaysConfigure
    }

    /// <summary>
    /// Configuration options type
    /// </summary>
    public enum ConfigurationOptionType
    {
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
        Object
    }

    /// <summary>
    /// Representsa feature that can be configured which is not 
    /// </summary>
    public interface IFeature
    {
	    /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        object Configuration { get; set; }

	    /// <summary>
        /// Gets the configuration type
        /// </summary>
        Type ConfigurationType { get; }

	    /// <summary>
        /// Get the description of the feature
        /// </summary>
        string Description { get; }

	    /// <summary>
        /// Gets the flags for this feature
        /// </summary>
        FeatureFlags Flags { get; }

	    /// <summary>
        /// Get the grouping in the configuration
        /// </summary>
        string Group { get; }

	    /// <summary>
        /// Gets the name of the feature
        /// </summary>
        string Name { get; }

	    /// <summary>
        /// Create the necessary tasks to configure the feature
        /// </summary>
        IEnumerable<IConfigurationTask> CreateInstallTasks();

	    /// <summary>
        /// Create uninstallation tasks
        /// </summary>
        IEnumerable<IConfigurationTask> CreateUninstallTasks();

	    /// <summary>
        /// Returns true if the configuration supplied is configured for this feature
        /// </summary>
        FeatureInstallState QueryState(SanteDBConfiguration configuration);
    }
}
