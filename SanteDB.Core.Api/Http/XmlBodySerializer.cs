/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a body serializer that uses XmlSerializer
    /// </summary>
    internal class XmlBodySerializer : IBodySerializer
    {
        // Serializers
        private static Dictionary<Type, XmlSerializer> m_serializers = new Dictionary<Type, XmlSerializer>();

        // Xml types
        private static List<Type> m_xmlTypes = null;

        // Serializer
        private XmlSerializer m_serializer;

        // Type
        private Type m_type;

        /// <summary>
        /// Creates a new body serializer
        /// </summary>
        public XmlBodySerializer(Type type, params Type[] extraTypes)
        {
            this.m_type = type;
            if (!m_serializers.TryGetValue(type, out this.m_serializer))
            {
                this.m_serializer = new XmlSerializer(type, extraTypes);
                lock (m_serializers)
                    if (!m_serializers.ContainsKey(type))
                        m_serializers.Add(type, this.m_serializer);
            }
        }

        #region IBodySerializer implementation

        /// <summary>
        /// Serialize the object
        /// </summary>
        public void Serialize(System.IO.Stream s, object o)
        {
            if (o.GetType() == this.m_type)
                this.m_serializer.Serialize(s, o);
            else // Slower
            {
                XmlSerializer xsz = new XmlSerializer(o.GetType(), (o as Bundle)?.Item.Select(i => i.GetType()).Distinct().ToArray() ?? new Type[0]);
                xsz.Serialize(s, o);
            }
        }

        /// <summary>
        /// Serialize the reply stream
        /// </summary>
        public object DeSerialize(System.IO.Stream s)
        {
            XmlSerializer serializer = null;
            using (XmlReader bodyReader = XmlReader.Create(s))
            {
                while (bodyReader.NodeType != XmlNodeType.Element)
                    bodyReader.Read();

                // Service fault?
                // Find candidate type
                if (this.m_serializer.CanDeserialize(bodyReader))
                    serializer = this.m_serializer;

                else if (bodyReader.LocalName == "RestServiceFault" &&
                   bodyReader.NamespaceURI == "http://santedb.org/fault")
                    serializer = new XmlSerializer(Type.GetType("SanteDB.Rest.Common.Fault.RestServiceFault, SanteDB.Rest.Common"));
                else
                {
                    Type eType = m_serializers.FirstOrDefault(o => o.Value.CanDeserialize(bodyReader)).Key;

                    if (m_xmlTypes == null)
                        m_xmlTypes = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes().Where(o => o.GetTypeInfo().GetCustomAttribute<XmlRootAttribute>() != null).ToList();
                    if (eType == null)
                        eType = m_xmlTypes.FirstOrDefault(o => o.GetTypeInfo().GetCustomAttribute<XmlRootAttribute>()?.ElementName == bodyReader.LocalName &&
                            o.GetTypeInfo().GetCustomAttribute<XmlRootAttribute>()?.Namespace == bodyReader.NamespaceURI);
                    if (eType == null)
                        throw new KeyNotFoundException($"Could not determine how to de-serialize {bodyReader.NamespaceURI}#{bodyReader.Name}");
                    if (!m_serializers.TryGetValue(eType, out serializer))
                    {
                        serializer = new XmlSerializer(eType);
                        lock (m_serializers)
                            if (!m_serializers.ContainsKey(eType))
                                m_serializers.Add(eType, serializer);
                    }
                }
                return serializer.Deserialize(bodyReader);
            }
        }

        #endregion IBodySerializer implementation
    }
}