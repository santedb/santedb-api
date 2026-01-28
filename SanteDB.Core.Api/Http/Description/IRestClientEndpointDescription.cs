/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using System;

namespace SanteDB.Core.Http.Description
{
    /// <summary>
    /// REST based client endpoint description
    /// </summary>
    public interface IRestClientEndpointDescription
    {
        /// <summary>
        /// Gets the address of the endpoint
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets or sets the timeout that a client will wait for a response from the server. Reading the response body stream can take longer than this value without throwing a <see cref="TimeoutException"/>.
        /// </summary>
        TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout that a client will wait for the entire operation, including reading and processing the response body, before timing out and throwing a <see cref="TimeoutException"/>.
        /// </summary>
        TimeSpan? ReceiveTimeout { get; set; }
    }
}