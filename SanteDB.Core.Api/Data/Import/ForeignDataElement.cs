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
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a single data element from a foreign data element
    /// </summary>
    [XmlType(nameof(ForeignDataElement), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElement : ForeignDataObject
    {

        /// <summary>
        /// Gets the type of the data element
        /// </summary>
        [XmlElement("type"), JsonProperty("type")]
        public TypeReferenceConfiguration Type { get; set; }

        /// <summary>
        /// Gets or sets an example of the data in this column
        /// </summary>
        [XmlElement("sample"),
            JsonProperty("sample")]
        public string SampleValue { get; set; }
    }
}