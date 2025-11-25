/*
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SanteDB.Core.i18n;

namespace SanteDB.Core.ViewModel.Description
{

    /// <summary>
    /// Represents model descriptions
    /// </summary>
    [XmlType(nameof(TypeModelDescription), Namespace = "http://santedb.org/model/view")]
    [ExcludeFromCodeCoverage]
    public class TypeModelDescription : PropertyContainerDescription
    {

        /// <summary>
        /// Initialize the type mode description
        /// </summary>
        internal void Initialize(ViewModelDescription parent)
        {
            this.PackageName = this.PackageName ?? parent.Name;
            for (int i = 0; i < this.Properties?.Count; i++)
            {
                this.Properties[i]?.Initialize(this);
            }

            this.ProcessBaseRefs(this, parent);
        }

        /// <summary>
        /// Process base refs
        /// </summary>
        /// <param name="parent">The parent description file in which the processing of the parents should occur</param>
        private void ProcessBaseRefs(TypeModelDescription type, ViewModelDescription parent)
        {
            if (!String.IsNullOrEmpty(type.Base))
            {
                var baseDef = parent.TypeModelDefinitions.FirstOrDefault(t => type.Base.Equals(t.Name, StringComparison.OrdinalIgnoreCase)) ?? 
                    parent.TypeModelDefinitions.FirstOrDefault(o => type.Base.Equals(o.TypeName, StringComparison.OrdinalIgnoreCase) && String.IsNullOrEmpty(o.Name)) ??
                    parent.TypeModelDefinitions.FirstOrDefault(o=>$"{o.PackageName}.{o.TypeName}".Equals(type.Base, StringComparison.OrdinalIgnoreCase) && String.IsNullOrEmpty(o.Name));
                if(baseDef == null)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.OBJECT_NOT_FOUND, type.Base));
                }
                this.Properties.AddRange(baseDef.Properties.Where(p => !this.Properties.Any(tp => p.Name == tp.Name)).ToArray());
                this.All = this.All ?? baseDef.All;
                this.ProcessBaseRefs(baseDef, parent);
            }
        }

        /// <summary>
        /// Gets or sets the base reference or type
        /// </summary>
        [XmlAttribute("base")]
        public string Base { get; set; }

        /// <summary>
        /// Gets or sets the name of the type model
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Name of the type to be loaded
        /// </summary>
        [XmlAttribute("type")]
        public string TypeName { get; set; }

        /// <summary>
        /// Owner name
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Get the name of the container
        /// </summary>
        internal override string GetName()
        {
            return this.TypeName;
        }

    }
}