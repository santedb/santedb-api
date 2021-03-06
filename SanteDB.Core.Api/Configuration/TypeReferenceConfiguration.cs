﻿/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a class that can be used to reference types in configuration files
    /// </summary>
    [XmlType(nameof(TypeReferenceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class TypeReferenceConfiguration
    {

        // The type
        private Type m_type;

        // Type
        private string m_typeXml;

        /// <summary>
        /// Represents a type reference configuration
        /// </summary>
        public TypeReferenceConfiguration()
        {

        }

        /// <summary>
        /// Create a new type reference from string
        /// </summary>
        public TypeReferenceConfiguration(string typeAqn)
        {
            this.TypeXml = typeAqn;
        }

        /// <summary>
        /// Gets the type operation
        /// </summary>
        public TypeReferenceConfiguration(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public String TypeXml 
        {
            get => this.m_typeXml;
            set
            {
                if (String.Equals(this.m_typeXml, value))
                    this.m_type = null;
                this.m_typeXml = value;
            }
        }

        /// <summary>
        /// Gets the type
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type Type
        {
            get
            {
                if (this.m_type == null)
                    this.m_type = Type.GetType(this.TypeXml);
                return this.m_type;
            }
            set
            {
                this.m_type = value;
                this.m_typeXml = value?.AssemblyQualifiedName;
            }
        }
    }
}
