﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core.Interop;
using System;

namespace SanteDB.Core.Interop
{
    /// <summary>
    /// Represents an SanteDB API endpoint
    /// </summary>
    public interface IApiEndpointProvider
    {
        /// <summary>
        /// Gets the service type
        /// </summary>
        ServiceEndpointType ApiType { get; }

        /// <summary>
        /// Service URL
        /// </summary>
        String[] Url { get; }

        /// <summary>
        /// Capabilities
        /// </summary>
        ServiceEndpointCapabilities Capabilities { get; }
    }
}
