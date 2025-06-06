﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Configuration for data quality configuration
    /// </summary>
    [XmlType(nameof(DataQualityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataQualityConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Gets or sets the rule sets for the data quality engine
        /// </summary>
        [XmlElement("ruleSet")]
        public List<DataQualityRulesetConfiguration> RuleSets { get; set; }

        /// <summary>
        /// When true indicates that data quality issues should be tagged on objects. 
        /// </summary>
        [DisplayName("Tag Issues"), Description("When set to true, instructs the data quality rule service to tag objects with a data quality extension. This may increase bandwidth and storage requirements")]
        [XmlAttribute("tagIssues")]
        public bool TagDataQualityIssues { get; set; }

    }
}
