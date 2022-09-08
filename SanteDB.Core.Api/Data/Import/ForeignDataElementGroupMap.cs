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
using SanteDB.Core.Configuration;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A class which describes how the components of a <see cref="ForeignDataElementGroup"/> can be imported
    /// </summary>
    [XmlType(nameof(ForeignDataElementGroupMap), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElementGroupMap : ForeignDataObjectMap
    {

        /// <summary>
        /// Gets the target type 
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public TypeReferenceConfiguration TargetType { get; set; }

        /// <summary>
        /// Gets the child or element maps
        /// </summary>
        [XmlArray("map"), XmlArrayItem("element", typeof(ForeignDataElementMap)), XmlArrayItem("group", typeof(ForeignDataElementGroupMap)),
            JsonProperty("map")]
        public List<ForeignDataObjectMap> ObjectMap { get; set; }

    }
}