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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Core.ViewModel.Description
{
    /// <summary>
    /// Represents a refined message model
    /// </summary>
    [XmlType(nameof(ViewModelDescription), Namespace = "http://santedb.org/model/view")]
    [XmlRoot("ViewModel", Namespace = "http://santedb.org/model/view")]
    [ExcludeFromCodeCoverage]
    public class ViewModelDescription
    {


        // Initialized?
        private bool m_isInitialized = false;
        // Description lookup
        private Dictionary<String, PropertyContainerDescription> m_description = new Dictionary<String, PropertyContainerDescription>();
        // Root type names
        private static Dictionary<Type, String> m_rootTypeNames = new Dictionary<Type, string>();
        // Serializer
        private static XmlSerializer s_xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(ViewModelDescription));

        /// <summary>
        /// The lock object
        /// </summary>
        protected object m_lockObject = new object();

        /// <summary>
        /// Defaut ctor
        /// </summary>
        public ViewModelDescription()
        {
            this.TypeModelDefinitions = new List<TypeModelDescription>();
            this.Include = new List<string>();
        }

        /// <summary>
        /// Includes
        /// </summary>
        [XmlElement("include")]
        public List<String> Include { get; set; }

        /// <summary>
        /// Gets or sets the name of the view model description
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the models which are to be defined in the model
        /// </summary>
        [XmlElement("type")]
        public List<TypeModelDescription> TypeModelDefinitions { get; set; }

        /// <summary>
        /// Load the specified view model description
        /// </summary>
        public static ViewModelDescription Load(Stream stream, string name = null)
        {
            var retVal = s_xsz.Deserialize(stream) as ViewModelDescription;
            retVal.Name = retVal.Name ?? name;
            return retVal;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize()
        {
            if (!this.m_isInitialized)
            {
                foreach (var itm in this.TypeModelDefinitions)
                {
                    itm.Initialize(this);
                }
            }
        }

        /// <summary>
        /// Find description based on name
        /// </summary>
        public PropertyContainerDescription FindDescription(String name)
        {
            PropertyContainerDescription value = null;
            if (!this.m_description.TryGetValue(name, out value))
            {
                value = this.TypeModelDefinitions.Find(o => o.TypeName == name) ?? this.TypeModelDefinitions.Find(o => o.Name == name);
                lock (this.m_lockObject)
                {
                    if (!this.m_description.ContainsKey(name))
                    {
                        this.m_description.Add(name, value);
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// Find description
        /// </summary>
        public PropertyContainerDescription FindDescription(Type rootType)
        {
            PropertyContainerDescription retVal = null;

            string rootTypeName = this.GetTypeName(rootType);

            // Type name
            if (!this.m_description.TryGetValue(rootTypeName, out retVal))
            {
                retVal = this.TypeModelDefinitions.FirstOrDefault(o => o.TypeName == rootTypeName && String.IsNullOrEmpty(o.Name));
                String typeName = rootTypeName;
                // Children from the heirarchy
                while (rootType != typeof(IdentifiedData) && retVal == null)
                {
                    rootType = rootType.BaseType;
                    if (rootType == null)
                    {
                        break;
                    }

                    typeName = this.GetTypeName(rootType);

                    if (!this.m_description.TryGetValue(typeName, out retVal))
                    {
                        retVal = this.TypeModelDefinitions.FirstOrDefault(o => o.TypeName == typeName && String.IsNullOrEmpty(o.Name));
                    }
                }

                lock (this.m_lockObject)
                {
                    if (!this.m_description.ContainsKey(rootTypeName))
                    {
                        this.m_description.Add(rootTypeName, retVal);
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Get type name
        /// </summary>
        private string GetTypeName(Type rootType)
        {
            string rootTypeName = null;
            if (!m_rootTypeNames.TryGetValue(rootType, out rootTypeName))
            {
                rootTypeName = rootType.GetCustomAttribute<XmlTypeAttribute>()?.TypeName ??
                                           rootType.Name;
                lock (m_rootTypeNames)
                {
                    if (!m_rootTypeNames.ContainsKey(rootType))
                    {
                        m_rootTypeNames.Add(rootType, rootTypeName);
                    }
                }
            }
            return rootTypeName;
        }

        /// <summary>
        /// Find description
        /// </summary>
        public PropertyContainerDescription FindDescription(String propertyName, PropertyContainerDescription context)
        {
            if (propertyName == null)
            {
                return null;
            }

            PropertyContainerDescription retVal = null;
            String pathName = propertyName;
            var pathContext = context;
            while (pathContext != null)
            {
                pathName = pathContext.GetName() + "." + pathName;
                pathContext = pathContext.Parent;
            }

            if (!this.m_description.TryGetValue(pathName, out retVal))
            {

                // Find the property information
                retVal = context?.FindProperty(propertyName);
                if (retVal == null)
                {
                    retVal = context?.FindProperty("*");
                }

                lock (this.m_lockObject)
                {
                    if (!this.m_description.ContainsKey(pathName))
                    {
                        this.m_description.Add(pathName, retVal);
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Merge several view model descriptions into one
        /// </summary>
        public static ViewModelDescription Merge(IEnumerable<ViewModelDescription> viewModels)
        {
            ViewModelDescription retVal = null;
            foreach (var itm in viewModels)
            {
                if (retVal == null)
                {
                    retVal = itm;
                }
                else
                {
                    MergeInternal(itm, retVal);
                }
            }

            return retVal;
        }


        /// <summary>
        /// Merge internal
        /// </summary>
        private static void MergeInternal(ViewModelDescription victim, ViewModelDescription merged)
        {
            foreach (var td in victim.TypeModelDefinitions)
            {
                var mergeModel = merged.TypeModelDefinitions.FirstOrDefault(o => o.TypeName == td.TypeName && o.Name == td.Name);
                if (mergeModel == null)
                {
                    merged.TypeModelDefinitions.Add(td);
                }
                else
                {
                    MergeInternal(td, mergeModel);
                }
            }
        }

        /// <summary>
        /// Merge internal
        /// </summary>
        private static void MergeInternal(PropertyContainerDescription victim, PropertyContainerDescription merged)
        {
            if (victim.All.GetValueOrDefault() && !merged.All.GetValueOrDefault())
            {
                merged.All = victim.All;
            }

            if (victim.Ref != merged.Ref && merged.Ref == null)
            {
                merged.Ref = victim.Ref;
            }

            if ((victim is PropertyModelDescription) &&
                (victim as PropertyModelDescription).Action != SerializationBehaviorType.Default &&
                (victim as PropertyModelDescription).Action < (merged as PropertyModelDescription)?.Action)
            {
                (merged as PropertyModelDescription).Action = (victim as PropertyModelDescription).Action;
            }

            foreach (var td in victim.Properties)
            {
                var mergeModel = merged.FindProperty(td.Name);
                if (mergeModel == null)
                {
                    merged.Properties.Add(td);
                }
                else
                {
                    MergeInternal(td, mergeModel);
                }
            }

        }
    }
}
