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
using Newtonsoft.Json;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Represents a definition of how to transform from a source <see cref="IForeignDataFormat"/>
    /// </summary>
    [XmlType(nameof(ForeignDataMap), Namespace = "http://santedb.org/import")]
    [XmlRoot(nameof(ForeignDataMap), Namespace = "http://santedb.org/import")]
    [JsonObject(nameof(ForeignDataMap))]
    public class ForeignDataMap : IIdentifiedResource
    {

        private static XmlSerializer s_serializer = new XmlSerializer(typeof(ForeignDataMap));

        /// <summary>
        /// Gets or sets the unique identifier for the element
        /// </summary>
        [XmlElement("id"), JsonProperty("id")]
        public Guid? Key { get; set; }

        /// <summary>
        /// Priority this definition has
        /// </summary>
        [XmlAttribute("priority"), JsonProperty("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the name of the element
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the map
        /// </summary>
        [XmlElement("description"), JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters 
        /// </summary>
        [XmlArray("parameters"), XmlArrayItem("add"), JsonProperty("parameters")]
        public List<ForeignDataMapParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the data maps
        /// </summary>
        [XmlArray("maps"), XmlArrayItem("map"), JsonProperty("maps")]
        public List<ForeignDataObjectMap> Maps { get; set; }

        /// <summary>
        /// Get the tags for the map
        /// </summary>
        [XmlElement("tag"), JsonProperty("tag")]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the creation time
        /// </summary>
        [XmlElement("creationTime"), JsonProperty("creationTime")]
        public DateTimeOffset ModifiedOn { get; set; }

        /// <summary>
        /// Load the foreign data map from the specified <paramref name="sourceStream"/>
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public static ForeignDataMap Load(Stream sourceStream)
        {
            return s_serializer.Deserialize(sourceStream) as ForeignDataMap;
        }

        /// <summary>
        /// Validate this mapping definition
        /// </summary>
        /// <returns>The list of detected issues</returns>
        public IEnumerable<ValidationResultDetail> Validate()
        {
            if (this.Maps?.Any() != true)
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, "No Maps Defined", null, String.Empty);
            }
            foreach (var m in this.Maps)
            {
                foreach (var itm in m.Validate())
                {
                    yield return itm;
                }
            }
        }

        /// <summary>
        /// Save the map
        /// </summary>
        public void Save(Stream ms)
        {
            s_serializer.Serialize(ms, this);
        }
    }
}
