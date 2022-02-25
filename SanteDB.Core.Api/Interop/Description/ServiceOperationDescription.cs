/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using System.Collections.Generic;
using System.Net;

namespace SanteDB.Core.Interop.Description
{
    /// <summary>
    /// A single operation on the service
    /// </summary>
    public class ServiceOperationDescription
    {

        /// <summary>
        /// Creates a new service description
        /// </summary>
        public ServiceOperationDescription(string verb, string path, string[] acceptsContentType, bool requiresAuth)
        {
            this.Responses = new Dictionary<HttpStatusCode, ResourceDescription>();
            this.Parameters = new List<OperationParameterDescription>();
            this.Verb = verb;
            this.Path = path;
            this.Accepts = this.Produces = acceptsContentType;
            this.Tags = new List<string>();
            this.RequiresAuth = requiresAuth;
        }

        /// <summary>
        /// Gets the verb for this description
        /// </summary>
        public string Verb { get; }

        /// <summary>
        /// Gets the responses
        /// </summary>
        public IDictionary<HttpStatusCode, ResourceDescription> Responses { get; }

        /// <summary>
        /// Gets the parameters
        /// </summary>
        public IList<OperationParameterDescription> Parameters { get; }

        /// <summary>
        /// Gets the path description
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the type of content that this operation accepts
        /// </summary>
        public IEnumerable<string> Accepts { get; }

        /// <summary>
        /// Gets the type of content that this operation produces
        /// </summary>
        public IEnumerable<string> Produces { get; }

        /// <summary>
        /// Gets the tags for this object
        /// </summary>
        public IList<string> Tags { get; }

        /// <summary>
        /// Requires authorization
        /// </summary>
        public bool RequiresAuth { get; }

    }
}