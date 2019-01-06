﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: justin
 * Date: 2018-11-26
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a generic claim
    /// </summary>
    public class SanteDBClaim : IClaim
    {

        /// <summary>
        /// Creates a new generic claim
        /// </summary>
        public SanteDBClaim(String type, String value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Gets the type
        /// </summary>
        public string Type {get;}

        /// <summary>
        /// Gets the value of the claim
        /// </summary>
        public string Value { get; }
    }
}