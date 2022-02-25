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
    /// Provider for name and place aliasing
    /// </summary>
    /// <remarks>
    /// Some implementations of the SanteDB engine allow for searching of data based on aliases. This aliasing
    /// allows SanteDB to map an inbound query parameter like 'Bob' to 'Robert' or 'Will' to 'Bill' and 'William'.
    /// The service is responsible for providing the known aliases for each name into the SanteDB infrastructure, and
    /// these can be accessed using the HDSI extended query filter: <code>name.component.value=:(alias|Bob)&gt;=1.0</code>
    /// which indicates that the aliases of the stored name should match Bob.
    /// </remarks>
    [System.ComponentModel.Description("Name Alias Provider")]
    public interface IAliasProvider : IServiceImplementation
    {

        /// <summary>
        /// Gets the known alias names and score for the alias 
        /// </summary>
        IEnumerable<ComponentAlias> GetAlias(String name);

        /// <summary>
        /// Add an alias to the alias provider
        /// </summary>
        void AddAlias(String name, String alias, double weight);

        /// <summary>
        /// Remove the specified alias 
        /// </summary>
        void RemoveAlias(String name, String alias);

        /// <summary>
        /// Get all aliases
        /// </summary>
        IDictionary<String, IEnumerable<ComponentAlias>> GetAllAliases(String filter, int offset, int? count, out int totalResults);
    }
}
