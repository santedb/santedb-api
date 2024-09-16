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
using SanteDB.Core.Services;
using System;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Defines a structure for a single atomic configuration task
    /// </summary>
    /// <remarks><para>The configuration task represents a single operation which modifies the execution or configuration environment to enable or remove
    /// a particular feature to/from the context. Individual configuration tasks can be disabled by administrators, therefore common task operations (like enabling a service
    /// and adding its configuration section) should be grouped together.</para></remarks>
    public interface IConfigurationTask : IReportProgressChanged
    {
        /// <summary>
        /// Get description of the task
        /// </summary>
        /// <remarks>This value is shown in the user interface to the administrator prior to the task being executed. It is recommended implementers
        /// of this interface include detailed information about what the configuration task is doing/changing in the context in which the configuration
        /// is being set.</remarks>
        string Description { get; }

        /// <summary>
        /// Gets the feature that this task configures
        /// </summary>
        IFeature Feature { get; }

        /// <summary>
        /// Get the name of the task to show on the user interface
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execute the configuration task on <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration against which the task should be executed</param>
        /// <returns>True if the application of the configuration task was successful</returns>
        bool Execute(SanteDBConfiguration configuration);

        /// <summary>
        /// Rollback changes in the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration against which the configuration task should be rolled back</param>
        /// <returns>True if the rollback operation was successful</returns>
        bool Rollback(SanteDBConfiguration configuration);

        /// <summary>
        /// Verify the task against <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration against which the task is to be validated</param>
        /// <returns>True if the task state is valid and can run</returns>
        bool VerifyState(SanteDBConfiguration configuration);
    }

    /// <summary>
    /// Defines a <see cref="IConfigurationTask"/> which has additional context such as a longer description and a help URL
    /// </summary>
    public interface IDescribedConfigurationTask : IConfigurationTask
    {
        /// <summary>
        /// Gets additional information about the task and what the task is performing
        /// </summary>
        string AdditionalInformation { get; }

        /// <summary>
        /// A link to an external website where the administrator can get more information about the task
        /// </summary>
        Uri HelpUri { get; }
    }
}