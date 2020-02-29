using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Represents a bundle data quality rule
    /// </summary>
    public class DataQualityBundleRule : BaseBusinessRulesService<Bundle>
    {

        /// <summary>
        /// Validate the specified bundle
        /// </summary>
        /// <param name="data">The data to be validated</param>
        /// <returns>The validated bundle rule</returns>
        public override List<DetectedIssue> Validate(Bundle data)
        {
            var issues = new List<DetectedIssue>();
            foreach (var itm in data.Item)
            {
                var breSvc = ApplicationServiceContext.Current.GetService(typeof(DataQualityBusinessRule<>).MakeGenericType(itm.GetType())) as IDataQualityBusinessRuleService;
                if(breSvc != null)
                    issues.AddRange(breSvc.Validate(itm));
            }
            return base.Validate(data).Union(issues).ToList();
        }

        /// <summary>
        /// Execute before insert rules
        /// </summary>
        public override Bundle BeforeInsert(Bundle data)
        {
            for(int i = 0; i < data.Item.Count; i++)
            {
                var itm = data.Item[i];
                var breSvc = ApplicationServiceContext.Current.GetService(typeof(DataQualityBusinessRule<>).MakeGenericType(itm.GetType())) as IDataQualityBusinessRuleService;
                if(breSvc != null)
                    data.Item[i] = breSvc.BeforeInsert(itm) as IdentifiedData;
            }
            return base.BeforeInsert(data);
        }

        /// <summary>
        /// Execute before update rules
        /// </summary>
        public override Bundle BeforeUpdate(Bundle data)
        {
            for (int i = 0; i < data.Item.Count; i++)
            {
                var itm = data.Item[i];
                var breSvc = ApplicationServiceContext.Current.GetService(typeof(DataQualityBusinessRule<>).MakeGenericType(itm.GetType())) as IDataQualityBusinessRuleService;
                if (breSvc != null)
                    data.Item[i] = breSvc.BeforeUpdate(itm) as IdentifiedData;
            }
            return base.BeforeUpdate(data);
        }

    }
}
