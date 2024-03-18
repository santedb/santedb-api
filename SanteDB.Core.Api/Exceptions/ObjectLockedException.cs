/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Indicates an object is already locked and the action cannot continue
    /// </summary>
    public class ObjectLockedException : Exception
    {
        /// <summary>
        /// Get the user which locked the object
        /// </summary>
        public String LockedUser { get; }

        /// <summary>
        /// Object is locked
        /// </summary>
        public ObjectLockedException() : base("Object locked")
        {
        }

        /// <summary>
        /// Object has been locked
        /// </summary>
        public ObjectLockedException(String lockUser) : base($"Object Locked by {lockUser}")
        {
            this.LockedUser = lockUser;
            this.Data.Add("user", lockUser);
        }
    }
}