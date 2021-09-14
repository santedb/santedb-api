using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public DataQualityRulesetConfiguration GetRuleSet(string name)
        {
            return this.m_configuration.RuleSets.FirstOrDefault(o => o.Name == name);
        }

        /// <summary>
        /// Get all rule sets
        /// </summary>
        public IEnumerable<DataQualityRulesetConfiguration> GetRuleSets()
        {
            return this.m_configuration.RuleSets;
        }

        /// <summary>
        /// Get all rules for the specified type
        /// </summary>
        public IEnumerable<DataQualityResourceConfiguration> GetRulesForType<T>()
        {
            return this.m_configuration.RuleSets.SelectMany(o => o.Resources).Where(r => r.ResourceType == typeof(T));
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
