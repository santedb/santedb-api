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
 * Date: 2023-5-19
 */
using SanteDB.Core.Model;
using SanteDB.Core.Cdss;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Represents an exception with the CDSS engine
    /// </summary>
    public class CdssException : Exception
    {

        /// <summary>
        /// The name of the protocol data element 
        /// </summary>
        public const string LibraryDataName = "libraries";
        /// <summary>
        /// The name of the target data element
        /// </summary>
        public const string TargetDataName = "target";

        /// <summary>
        /// Gets the protocols which caused the exception
        /// </summary>
        public IEnumerable<ICdssProtocol> Protocols => this.Data[LibraryDataName] as IEnumerable<ICdssProtocol>;

        /// <summary>
        /// Gets the target which caused the exception
        /// </summary>
        public String Target => this.Data[TargetDataName].ToString();

        /// <summary>
        /// CDSS exception
        /// </summary>
        /// <param name="libraries">The protocols which were applied</param>
        /// <param name="target">The target of the CDSS call</param>
        /// <param name="cause">The cause of the exception</param>
        public CdssException(IEnumerable<ICdssLibrary> libraries, IdentifiedData target, Exception cause) : base($"Error executing CDSS rules against {target}", cause)
        {
            this.Data.Add(LibraryDataName, libraries);
            this.Data.Add(TargetDataName, target.ToString());
        }

        /// <summary>
        /// Creates a new instance of the CDSS exception
        /// </summary>
        /// <param name="libraries">The clinical protocols that were attempted to be applied</param>
        /// <param name="target">The target of the CDSS operation</param>
        public CdssException(IEnumerable<ICdssLibrary> libraries, IdentifiedData target) : this(libraries, target, null)
        {

        }
    }
}
