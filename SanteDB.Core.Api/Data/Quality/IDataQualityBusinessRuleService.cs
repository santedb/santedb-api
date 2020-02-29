using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Services;
using System;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Represents a basic data quality business rule service
    /// </summary>
    internal interface IDataQualityBusinessRuleService : IBusinessRulesService
    {

        /// <summary>
        /// Adds a data quality resource configuration to this business rules object
        /// </summary>
        void AddDataQualityResourceConfiguration(String ruleSetId, DataQualityResourceConfiguration configuration);

       
    }
}