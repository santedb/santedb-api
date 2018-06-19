﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * Date: 2017-9-1
 */
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Warehouse
{
    /// <summary>
    /// Represents the SQL for an actual query
    /// </summary>
    [XmlType(nameof(DatamartStoredQueryDefinition), Namespace = "http://santedb.org/warehousing"), JsonObject(nameof(DatamartStoredQueryDefinition))]
    public class DatamartStoredQueryDefinition
    {

        /// <summary>
        /// Provider identifier
        /// </summary>
        [XmlAttribute("provider"), JsonProperty("provider")]
        public string ProviderId { get; set; }

        /// <summary>
        /// The SQL 
        /// </summary>
        [XmlText, JsonProperty("sql")]
        public string Query { get; set; }
    }
}