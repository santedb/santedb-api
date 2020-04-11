/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration
{
    
    /// <summary>
    /// Represents a configuration task
    /// </summary>
    public interface IConfigurationTask : IReportProgressChanged
    {

        /// <summary>
        /// Get the name of the task
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Get description of the task
        /// </summary>
        String Description { get;  }

        /// <summary>
        /// Gets the feature that is being configured
        /// </summary>
        IFeature Feature { get; }

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
        /// Get information about the task
        /// </summary>
        Uri HelpUri { get; }

        /// <summary>
        /// Gets the additional information
        /// </summary>
        String AdditionalInformation { get; }
    }
}
