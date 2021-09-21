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

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a simple policy implemtnation
    /// </summary>
    public class GenericPolicy : IPolicy
    {

        /// <summary>
        /// Generic policy
        /// </summary>
        public GenericPolicy()
        {

        }

        /// <summary>
        /// Constructs a simple policy 
        /// </summary>
        public GenericPolicy(Guid key, String oid, String name, bool canOverride)
        {
            this.Key = key;
            this.Oid = oid;
            this.Name = name;
            this.CanOverride = canOverride;
            this.IsActive = true;
        }

        /// <summary>
        /// Gets the key
        /// </summary>
        public Guid Key
        {
            get; private set;
        }

        /// <summary>
        /// True if the policy can be overridden
        /// </summary>
        public bool CanOverride
        {
            get; private set;
        }

        /// <summary>
        /// Returns true if the policy is active
        /// </summary>
        public bool IsActive
        {
            get; private set;
        }

        /// <summary>
        /// Gets the name of the policy
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// Gets the oid of the policy
        /// </summary>
        public string Oid
        {
            get; private set;
        }
    }
}
