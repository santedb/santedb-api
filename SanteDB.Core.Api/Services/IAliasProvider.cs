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

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents a component alias
    /// </summary>
    public struct ComponentAlias
    {
        /// <summary>
        /// Represents component aliases
        /// </summary>
        /// <param name="alias">The alias of the name</param>
        /// <param name="relevance">The relevance of the name alias</param>
        public ComponentAlias(string alias, double relevance)
        {
            this.Alias = alias;
            this.Relevance = relevance;
        }

        /// <summary>
        /// Gets the aliased name
        /// </summary>
        public String Alias { get; private set; }

        /// <summary>
        /// Gets the relevance of the alias
        /// </summary>
        public double Relevance { get; private set; }

    }

    /// <summary>
    /// Represents a provider for aliases
    /// </summary>
    public interface IAliasProvider : IServiceImplementation
    {

        /// <summary>
        /// Gets the known alias names and score for the alias 
        /// </summary>
        IEnumerable<ComponentAlias> GetAlias(String name);
    }
}
