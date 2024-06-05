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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Business rule that calls other <see cref="DataQualityBusinessRule{TModel}"/> based on the contents of a bundle
    /// </summary>
    /// <remarks>
    /// This business rule wraps the insertion or update of a <see cref="Bundle"/> and calls individual data quality validation 
    /// rules for each of the objects contained withing the bundle provided.
    /// </remarks>
    public class DataQualityBundleRule : BaseBusinessRulesService<Bundle>
    {

        private Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityBundleRule));

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
                var breSvc = ApplicationServiceContext.Current.GetService(typeof(DataQualityBusinessRule<>).MakeGenericType(itm.GetType())) as IBusinessRulesService;
                if (breSvc != null)
                {
                    issues.AddRange(breSvc.Validate(itm));
                }
            }
            return base.Validate(data).Union(issues).ToList();
        }

        /// <summary>
        /// Execute before insert rules
        /// </summary>
        public override Bundle BeforeInsert(Bundle data)
        {

            for (int i = 0; i < data.Item.Count; i++)
            {
                var itm = data.Item[i];
                var breSvc = ApplicationServiceContext.Current.GetService(typeof(DataQualityBusinessRule<>).MakeGenericType(itm.GetType())) as IBusinessRulesService;
                if (breSvc != null)
                {
                    data.Item[i] = breSvc.BeforeInsert(itm) as IdentifiedData;
                    this.m_tracer.TraceVerbose("DataQualityRule: BeforeInsert: Result {0}", data.Item[i]);
                }
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
                var breSvc = ApplicationServiceContext.Current.GetService(typeof(DataQualityBusinessRule<>).MakeGenericType(itm.GetType())) as IBusinessRulesService;
                if (breSvc != null)
                {
                    data.Item[i] = breSvc.BeforeUpdate(itm) as IdentifiedData;
                    this.m_tracer.TraceVerbose("DataQualityRule: BeforeUpdate: Result {0}", data.Item[i]);
                }
            }
            return base.BeforeUpdate(data);
        }

    }
}
