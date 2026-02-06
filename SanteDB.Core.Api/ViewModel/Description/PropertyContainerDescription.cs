/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.ViewModel.Description
{
    /// <summary>
    /// Property container description
    /// </summary>
    [XmlType(nameof(PropertyContainerDescription), Namespace = "http://santedb.org/model/view")]
    [ExcludeFromCodeCoverage]
    public abstract class PropertyContainerDescription
    {

        // Property models by name
        private Dictionary<String, PropertyModelDescription> m_properties = new Dictionary<string, PropertyModelDescription>();


        /// <summary>
        /// Gets the name of the object
        /// </summary>
        internal abstract String GetName();

        /// <summary>
        /// Type model description
        /// </summary>
        public PropertyContainerDescription()
        {
            this.Properties = new List<PropertyModelDescription>();
        }


        /// <summary>
        /// Property container description
        /// </summary>
        [XmlIgnore]
        public PropertyContainerDescription Parent { get; protected set; }

        /// <summary>
        /// Identifies the properties to be included
        /// </summary>
        [XmlElement("property")]
        public List<PropertyModelDescription> Properties { get; set; }

        /// <summary>
        /// Whether to retrieve all children serialized for XML
        /// </summary>
        // HACK: This is done because XML serializer can't handle nullables
        [XmlAttribute("all")]
        public string AllXml
        {
            get => this.All.HasValue ? XmlConvert.ToString(this.All.Value) : null;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    this.All = null;
                }
                else
                {
                    this.All = XmlConvert.ToBoolean(value);
                }
            }
        }

        /// <summary>
        /// Property for nullable value of all
        /// </summary>
        [XmlIgnore]
        public bool? All { get; set; }

        /// <summary>
        /// Gets the reference to use
        /// </summary>
        [XmlAttribute("ref")]
        public String Ref { get; set; }

        /// <summary>
        /// Find property
        /// </summary>
        public PropertyModelDescription FindProperty(String name)
        {
            PropertyModelDescription model = null;
            if (!this.m_properties.TryGetValue(name, out model))
            {
                var arrSearch = this.Properties.ToArray();
                model = arrSearch.FirstOrDefault(o => o.Name == name);
                lock (this.m_properties)
                {
                    if (!this.m_properties.ContainsKey(name))
                    {
                        this.m_properties.Add(name, model);
                    }
                }
            }
            return model;
        }

    }
}
