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
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// A business rule that applies user defined data quality rules from the <see cref="IDataQualityConfigurationProviderService"/>
    /// </summary>
    /// <remarks>
    /// <para>This business rule template allows users to describe data quality rules (for example, using <see href="https://help.santesuite.org/operations/server-administration/host-configuration-file/data-quality-services#configuring-data-quality-rules">the application configuration for data quality rules</see>) 
    /// to be applied to incoming data. This service uses the data quality extension to then flag any warnings or informational issues (error issues result in the object being rejected). 
    /// These extensions are cleaned by the <see cref="DataQualityExtensionCleanJob"/> if the object no longer fails the quality rules.</para>
    /// </remarks>
    public class DataQualityBusinessRule<TModel> : BaseBusinessRulesService<TModel>
        where TModel : IdentifiedData
    {
        // Configuration provider
        private IDataQualityConfigurationProviderService m_configurationProvider;
        private readonly DataQualityConfigurationSection m_configuration;

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityBusinessRule<TModel>));

        /// <summary>
        /// Creates a new data quality business rule
        /// </summary>
        public DataQualityBusinessRule(IDataQualityConfigurationProviderService dataQualityConfigurationProviderService, IConfigurationManager configurationManager)
        {
            this.m_tracer.TraceVerbose("Business rule service for {0} created", typeof(TModel).Name);
            this.m_configurationProvider = dataQualityConfigurationProviderService;
            this.m_configuration = configurationManager.GetSection<DataQualityConfigurationSection>();
        }

        /// <summary>
        /// Before inserting, validation DQ
        /// </summary>
        public override TModel BeforeInsert(TModel data)
        {
            if (data is IExtendable extendable && this.m_configuration.TagDataQualityIssues)
            {
                extendable.TagDataQualityIssues();
            }
            return base.BeforeInsert(data);
        }

        /// <summary>
        /// Before update, validate DQ
        /// </summary>
        public override TModel BeforeUpdate(TModel data)
        {
            if (data is IExtendable extendable && this.m_configuration.TagDataQualityIssues)
            {
                extendable.TagDataQualityIssues();
            }
            return base.BeforeUpdate(data);
        }

        /// <summary>
        /// Validate the specified resource
        /// </summary>
        public override List<DetectedIssue> Validate(TModel data)
        {
            List<DetectedIssue> retVal = data.Validate().ToList();
            return base.Validate(data).Union(retVal).ToList();
        }
    }
}