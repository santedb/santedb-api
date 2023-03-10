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
 * Date: 2023-3-10
 */
using System.Collections.Generic;

namespace SanteDB.Core.Http.Description
{
    /// <summary>
    /// Represents a description of a service
    /// </summary>
    public interface IRestClientDescription
    {
        /// <summary>
        /// The messaging format for the accept header
        /// </summary>
        string Accept { get; }

        /// <summary>
        /// Gets or sets the binding for the service client.
        /// </summary>
        IRestClientBindingDescription Binding { get; }

        /// <summary>
        /// Gets or sets the endpoints for the client
        /// </summary>
        List<IRestClientEndpointDescription> Endpoint { get; }

        /// <summary>
        /// Gets whether a tracing is enabled.
        /// </summary>
        bool Trace { get; }

        /// <summary>
        /// Gets the proxy address
        /// </summary>
        string ProxyAddress { get; }
    }
}