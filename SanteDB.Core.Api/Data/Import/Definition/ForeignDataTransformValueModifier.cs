/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Represents a transform which can be applied against a source object 
    /// </summary>
    [XmlType(nameof(ForeignDataTransformValueModifier), Namespace = "http://santedb.org/import")]
    public class ForeignDataTransformValueModifier : ForeignDataValueModifier
    {

        /// <summary>
        /// Gets the type of transform to use
        /// </summary>
        [XmlAttribute("transformer"), JsonProperty("transformer")]
        public string Transformer { get; set; }

        /// <summary>
        /// Gets or sets the list of arguments
        /// </summary>
        [XmlArray("args"),
            XmlArrayItem("int", typeof(Int32)),
            XmlArrayItem("string", typeof(String)),
            XmlArrayItem("bool", typeof(Boolean)),
            XmlArrayItem("dateTime", typeof(DateTime)),
            JsonProperty("args")]
        public List<Object> Arguments { get; set; }

        /// <summary>
        /// Validate this transformer exists
        /// </summary>
        internal bool Validate()
        {
            return ForeignDataImportUtil.Current.TryGetElementTransformer(this.Transformer, out _);
        }
    }

}