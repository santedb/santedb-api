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
using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace SanteDB.Core.ViewModel.Description
{
    /// <summary>
    /// Property model description
    /// </summary>
    [XmlType(nameof(PropertyModelDescription), Namespace = "http://santedb.org/model/view")]
    [ExcludeFromCodeCoverage]
    public class PropertyModelDescription : PropertyContainerDescription
    {


        /// <summary>
        /// Initialize the parent structure
        /// </summary>
        public void Initialize(PropertyContainerDescription parent)
        {
            this.Parent = parent;
            foreach (var itm in this.Properties)
            {
                itm.Initialize(this);
            }
        }


        /// <summary>
        /// The property of the model
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or ssets the where classifiers
        /// </summary>
        [XmlAttribute("classifier")]
        public String Classifier { get; set; }

        /// <summary>
        /// Seriallization behavior
        /// </summary>
        [XmlAttribute("behavior")]
        public SerializationBehaviorType Action { get; set; }

        /// <summary>
        /// Get name 
        /// </summary>
        internal override string GetName()
        {
            return this.Name;
        }
    }
}