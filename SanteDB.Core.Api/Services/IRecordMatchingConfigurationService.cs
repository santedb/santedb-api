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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service 
    /// </summary>
    public interface IRecordMatchingConfigurationService : IServiceImplementation
    {

        /// <summary>
        /// Get the specified named configuration
        /// </summary>
        IRecordMatchingConfiguration GetConfiguration(String name);

        /// <summary>
        /// Saves the specified configuration to the configuration service
        /// </summary>
        IRecordMatchingConfiguration SaveConfiguration(IRecordMatchingConfiguration configuration);

        /// <summary>
        /// Gets the names of configurations in this provider
        /// </summary>
        IEnumerable<String> Configurations { get; }
    }
}
