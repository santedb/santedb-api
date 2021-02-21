/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
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