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
using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Services;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Data quality configuration provider service
    /// </summary>
    public interface IDataQualityConfigurationProviderService : IServiceImplementation
    {

        /// <summary>
        /// Get data quality rule set
        /// </summary>
        IEnumerable<DataQualityRulesetConfiguration> GetRuleSets();

        /// <summary>
        /// Get the rule set 
        /// </summary>
        DataQualityRulesetConfiguration GetRuleSet(string name);

        /// <summary>
        /// Save the specified ruleset
        /// </summary>
        DataQualityRulesetConfiguration SaveRuleSet(DataQualityRulesetConfiguration configuration);

        /// <summary>
        /// Get rule sets for the specified object
        /// </summary>
        IEnumerable<DataQualityResourceConfiguration> GetRulesForType<T>();
    }
}
