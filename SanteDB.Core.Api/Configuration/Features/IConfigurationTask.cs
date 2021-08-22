/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using System;
using SanteDB.Core.Services;

namespace SanteDB.Core.Configuration
{
    
    /// <summary>
    /// Represents a configuration task
    /// </summary>
    public interface IConfigurationTask : IReportProgressChanged
    {
	    /// <summary>
        /// Get description of the task
        /// </summary>
        string Description { get;  }

	    /// <summary>
        /// Gets the feature that is being configured
        /// </summary>
        IFeature Feature { get; }

	    /// <summary>
        /// Get the name of the task
        /// </summary>
        string Name { get; }

	    /// <summary>
        /// Execute the configuration task
        /// </summary>
        bool Execute(SanteDBConfiguration configuration);

	    /// <summary>
        /// Rollback changes in the specified configuration
        /// </summary>
        bool Rollback(SanteDBConfiguration configuration);

	    /// <summary>
        /// Verify the task prior to running
        /// </summary>
        bool VerifyState(SanteDBConfiguration configuration);
    }

    /// <summary>
    /// Represents a configuration task which is described
    /// </summary>
    public interface IDescribedConfigurationTask : IConfigurationTask
    {
	    /// <summary>
        /// Gets the additional information
        /// </summary>
        string AdditionalInformation { get; }

	    /// <summary>
        /// Get information about the task
        /// </summary>
        Uri HelpUri { get; }
    }
}
