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
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Get the ruleset list from configuration file
    /// </summary>
    class LegacyRulesetConfigurationProvider : IDataQualityConfigurationProviderService
    {

        // Configuration
        private DataQualityConfigurationSection m_configuration;

        /// <summary>
        /// Configuration manager
        /// </summary>
        public LegacyRulesetConfigurationProvider(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<DataQualityConfigurationSection>();
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Legacy Ruleset Provider";

        /// <summary>
        /// Get the total set of rules
        /// </summary>
        public DataQualityRulesetConfiguration GetRuleSet(string id)
        {
            return this.m_configuration.RuleSets.FirstOrDefault(o => o.Id == id);
        }

        /// <summary>
        /// Get all rule sets
        /// </summary>
        public IEnumerable<DataQualityRulesetConfiguration> GetRuleSets(bool includeObsoleted = false)
        {
            return this.m_configuration.RuleSets;
        }

        /// <summary>
        /// Get all rules for the specified type
        /// </summary>
        public IEnumerable<DataQualityResourceConfiguration> GetRulesForType<T>() => this.GetRulesForType(typeof(T));

        /// <summary>
        /// Get all rules for the specified type
        /// </summary>
        public IEnumerable<DataQualityResourceConfiguration> GetRulesForType(Type forType) => this.m_configuration.RuleSets.SelectMany(o => o.Resources).Where(r => r.ResourceType == forType);
        

        /// <inheritdoc/>
        public void RemoveRuleSet(string id)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Save the specified ruleset
        /// </summary>
        public DataQualityRulesetConfiguration SaveRuleSet(DataQualityRulesetConfiguration configuration)
        {
            throw new NotSupportedException();
        }
    }
}
