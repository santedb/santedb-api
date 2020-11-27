﻿/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2020-5-1
 */
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Represents a single data quality configuration for a specific resource
    /// </summary>
    [XmlType(nameof(DataQualityResourceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class DataQualityResourceConfiguration
    {

        /// <summary>
        /// Gets or sets the resource name
        /// </summary>
        [XmlAttribute("resource")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource
        /// </summary>
        [XmlIgnore]
        public Type ResourceType {
            get => new ModelSerializationBinder().BindToType(null, this.ResourceName);
            set
            {
                new ModelSerializationBinder().BindToName(value, out string asm, out string type);
                this.ResourceName = type;
            }
        }

        /// <summary>
        /// Gets or sets the assertions
        /// </summary>
        [XmlElement("assert")]
        public List<DataQualityResourceAssertion> Assertions { get; set; }

    }
}