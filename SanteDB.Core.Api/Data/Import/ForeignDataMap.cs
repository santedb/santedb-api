/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Defines how data can be imported from a data import source 
    /// </summary>
    [XmlType(nameof(ForeignDataMap), Namespace = "http://santedb.org/import")]
    [XmlRoot(nameof(ForeignDataMap), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataMap
    {

        /// <summary>
        /// Gets or sets the UUID for the map
        /// </summary>
        [XmlElement("uuid"), JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the map
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The source object from the foreign data description this map applies to
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public ForeignDataElementGroup Source { get; set; }

        /// <summary>
        /// Gets or sets the mapping definition for the object
        /// </summary>
        [XmlArray("map"),
            XmlArrayItem("element", typeof(ForeignDataElementMap)),
            XmlArrayItem("group", typeof(ForeignDataElementGroupMap)),
            JsonProperty("map")]
        public List<ForeignDataObjectMap> ObjectMap { get; set; }
    }
}
