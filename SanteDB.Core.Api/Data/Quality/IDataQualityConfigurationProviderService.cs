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
