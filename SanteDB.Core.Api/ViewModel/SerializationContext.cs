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
using SanteDB.Core.ViewModel.Description;
using SanteDB.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using SanteDB.Core.ViewModel.Json;

namespace SanteDB.Core.ViewModel
{
    /// <summary>
    /// Represents a serialization context
    /// </summary>
    public abstract class SerializationContext
    {
        // Serialization checks already created
        private static ConcurrentDictionary<Type, Object> m_cachedSerializationChecks = new ConcurrentDictionary<Type, object>();

        private readonly String[] ALWAYS_SERIALIZE =
        {
            "id",
            "classConcept",
            "negationInd",
            "statusConcept",
            "sequence",
            "version",
            "determinerConcept",
            "moodConcept",
            "operation"
        };


        // Object identifier
        private int m_objectId = 0;
        private int m_masterObjectId = 0;
        private Dictionary<String, MethodInfo> m_serializationCheck;

        // Element description
        private PropertyContainerDescription m_elementDescription;

        /// <summary>
        /// Gets the serialization context
        /// </summary>
        public SerializationContext(String propertyName, IViewModelSerializer context, Object instance)
        {
            this.PropertyName = propertyName;
            this.Parent = null;
            this.Instance = instance;
            this.Context = context;
            this.ViewModelDescription = this.Context.ViewModel;
            this.LoadedProperties = new Dictionary<Guid, HashSet<string>>();

            // Attempt to load from cache
            object check = null;
            if (instance != null && !m_cachedSerializationChecks.TryGetValue(instance.GetType(), out check))
            {
                this.m_serializationCheck = instance?.GetType().GetRuntimeProperties()
                    .Select(p => new { MethodInfo = p.DeclaringType.GetRuntimeMethod($"ShouldSerialize{p.Name}", new Type[0]), SerializationName = p.GetCustomAttributes<XmlElementAttribute>().FirstOrDefault()?.ElementName })
                    .Where(o => o.MethodInfo != null && !String.IsNullOrEmpty(o.SerializationName))
                    .ToDictionary(o => o.SerializationName, o => o.MethodInfo);
                m_cachedSerializationChecks.TryAdd(instance.GetType(), this.m_serializationCheck);
            }
            else if (check != null)
            {
                this.m_serializationCheck = (Dictionary<String, MethodInfo>)check;
            }
        }

        /// <summary>
        /// Gets the serialization context
        /// </summary>
        public SerializationContext(String propertyName, IViewModelSerializer context, Object instance, SerializationContext parent) : this(propertyName, context, instance)
        {
            this.Parent = parent;
            this.m_objectId = this.Root.m_masterObjectId++;
            this.LoadedProperties = parent?.LoadedProperties ?? new Dictionary<Guid, HashSet<string>>();
        }

        /// <summary>
        /// Get the debug view
        /// </summary>
        public string DebugView
        {
            get
            {
                var c = this;
                String retVal = String.Empty;
                while (c != null)
                {
                    retVal = "." + c.ToString() + retVal;
                    c = c.Parent;
                }
                return retVal;
            }
        }

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        public String PropertyName { get; private set; }

        /// <summary>
        /// Gets the view model serializer in context
        /// </summary>
        public IViewModelSerializer Context { get; private set; }

        /// <summary>
        /// Gets or sets the view model description of the current element
        /// </summary>
        public PropertyContainerDescription ElementDescription
        {
            get
            {
                if (this.m_elementDescription == null)
                {
                    var elementDescription = this.ViewModelDescription?.FindDescription(this.PropertyName, this.Parent?.ElementDescription);
                    if (elementDescription == null)
                    {
                        // Is the parent's applicable to this type?
                        if (this.Parent?.Instance.GetType().StripGeneric() == this.Instance.GetType())
                        {
                            elementDescription = this.Parent.ElementDescription;
                        }
                        else
                        {
                            elementDescription = this.ViewModelDescription?.FindDescription(this.Instance?.GetType().StripGeneric());
                        }
                    }

                    if (!String.IsNullOrEmpty(elementDescription?.Ref))
                    {
                        if (elementDescription.Ref.Equals("$type"))
                        {
                            elementDescription = this.ViewModelDescription?.FindDescription(this.Instance.GetType().GetSerializationName()) ?? elementDescription;
                        }
                        else
                        {
                            elementDescription = this.ViewModelDescription?.FindDescription(elementDescription.Ref) ?? elementDescription;
                        }
                    }
                    else if(elementDescription?.Properties?.Any() != true)
                    {
                        elementDescription = this.ViewModelDescription?.FindDescription(this.Instance.GetType().GetSerializationName()) ?? elementDescription;
                    }

                    this.m_elementDescription = elementDescription;
                }
                return this.m_elementDescription;
            }
        }

        /// <summary>
        /// Gets or sets the root view model description
        /// </summary>
        public ViewModelDescription ViewModelDescription { get; private set; }

        /// <summary>
        /// Gets the parent of the current serialization context
        /// </summary>
        public SerializationContext Parent { get; private set; }

        /// <summary>
        /// Gets or sets the instance value
        /// </summary>
        public Object Instance { get; private set; }

        /// <summary>
        /// Gets the root context
        /// </summary>
        public SerializationContext Root
        {
            get
            {
                var idx = this;
                while (idx.Parent != null)
                {
                    idx = idx.Parent;
                }

                return idx;
            }
        }

        /// <summary>
        /// Returns true when child property information should be force loaded
        /// </summary>
        public bool ShouldForceLoad(string childProperty, Guid? key)
        {
            var propertyDescription = this.ElementDescription?.FindProperty(childProperty) as PropertyModelDescription;
            if (propertyDescription == null)
            {
                var masterDescription = this.ViewModelDescription?.FindDescription(this.Instance.GetType().GetSerializationName());
                propertyDescription = masterDescription?.FindProperty(childProperty);
            }

            if (propertyDescription?.Action != SerializationBehaviorType.Always || this.Context.ForbidDelayLoad )
            {
                return false;
            }

            // Known miss targets
            HashSet<String> missProp = null;
            if (key.HasValue)
            {
                if (this.LoadedProperties.TryGetValue(key.Value, out missProp))
                {
                    if (missProp.Contains(childProperty))
                    {
                        return false;
                    }
                }
                else
                {
                    this.LoadedProperties.Add(key.Value, new HashSet<string>() { });
                }
            }

            return true;
        }

        /// <summary>
        /// Register that this property was missed
        /// </summary>
        public void RegisterMissTarget(String childProperty, Guid key)
        {
            HashSet<String> missProp = null;
            if (this.LoadedProperties.TryGetValue(key, out missProp))
            {
                if (!missProp.Contains(childProperty))
                {
                    missProp.Add(childProperty);
                }
            }
            else
            {
                this.LoadedProperties.Add(key, new HashSet<string>() { childProperty });
            }
        }
        /// <summary>
        /// Gets the current object identifier (from a JSON property perspective
        /// </summary>
        public int ObjectId { get { return this.m_objectId; } }


        /// <summary>
        /// Loaded properties
        /// </summary>
        public Dictionary<Guid, HashSet<String>> LoadedProperties { get; private set; }

        /// <summary>
        /// Gets the object id of the specified object from the parent instance if it exists 
        /// </summary>
        public int? GetParentObjectId(IdentifiedData data)
        {

            var idx = this;
            while (idx != null)
            {
                if (idx.Instance.GetType() == data.GetType() &&
                    (idx.Instance as IdentifiedData)?.Key.HasValue == true &&
                    data.Key.HasValue &&
                    (idx.Instance as IdentifiedData)?.Key.Value == data.Key.Value ||
                    idx.Instance == data)
                {
                    return idx.ObjectId;
                }

                idx = idx.Parent;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the object should be serialized based on the data at hand
        /// </summary>
        public bool ShouldSerialize(String childProperty)
        {
            var retVal = true;
            if (ALWAYS_SERIALIZE.Contains(childProperty))
            {
                return retVal;
            }

            var subPropertyDescription = this.ElementDescription?.FindProperty(childProperty);
            if (subPropertyDescription?.Action == SerializationBehaviorType.Never || // Sub-property is explicit - it should never be serialized
                this.ElementDescription?.All == false && subPropertyDescription == null) // This scope is not ALL and there is no explicit property
            {
                retVal = false;
            }
            else if (subPropertyDescription == null && (this.ElementDescription == null || !this.ElementDescription.All.HasValue)) // This scope is not defined so use the parent
            {
                // Parent is not set to all and does not explicitly call this property out
                retVal &= this.Parent?.ElementDescription?.All == true;
            }


            return retVal;
        }

        /// <summary>
        /// Property string
        /// </summary>
        public override string ToString()
        {
            return this.PropertyName;
        }
    }
}
