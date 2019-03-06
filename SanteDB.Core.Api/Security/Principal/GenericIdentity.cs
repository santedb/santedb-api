/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: justi
 * Date: 2019-1-12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Principal
{
    /// <summary>
    /// Represents a generic identity
    /// </summary>
    public class GenericIdentity : IIdentity
    {

        /// <summary>
        /// Represents a generic identity
        /// </summary>
        public GenericIdentity(String name)
        {
            this.Name = name;
        }

        /// <summary>
        /// The generic identity
        /// </summary>
        public GenericIdentity(String name, Boolean isAuthenticated, String authenticationType)
        {
            this.Name = name;
            this.IsAuthenticated = isAuthenticated;
            this.AuthenticationType = authenticationType;
        }

        /// <summary>
        /// Get the authentication type
        /// </summary>
        public string AuthenticationType { get; }

        /// <summary>
        /// Get whether is authenticated
        /// </summary>
        public bool IsAuthenticated { get; }

        /// <summary>
        /// Get name
        /// </summary>
        public string Name { get; }
    }
}
