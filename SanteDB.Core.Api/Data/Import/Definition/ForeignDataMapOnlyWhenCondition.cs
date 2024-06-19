﻿/*
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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// When condition
    /// </summary>
    [XmlType(nameof(ForeignDataMapOnlyWhenCondition), Namespace = "http://santedb.org/import")]
    public class ForeignDataMapOnlyWhenCondition : ForeignDataMapBase
    {
        /// <summary>
        /// Gets or sets the value that the <see cref="ForeignDataMapBase.Source"/>
        /// must equal 
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public List<string> Value { get; set; }

        /// <summary>
        /// Negation of the when condition (NOT)
        /// </summary>
        [XmlAttribute("negate"), JsonProperty("negate")]
        public bool Negation { get; set; }

        /// <summary>
        /// Gets or sets another column to compare to
        /// </summary>
        [XmlElement("refValue"), JsonProperty("refValue")]
        public string Other { get; set; }
    }
}