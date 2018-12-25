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
 * User: justin
 * Date: 2018-11-27
 */
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
    public sealed class TypeReferenceConfiguration
    {

        public TypeReferenceConfiguration()
        {

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
        [XmlAttribute("type")]
        public String TypeXml { get; set; }

        /// <summary>
        /// Gets the type
        /// </summary>
        [XmlIgnore]
        public Type Type
        {
            get => Type.GetType(this.TypeXml);
            set => this.TypeXml = value?.AssemblyQualifiedName;
        }
    }
}
