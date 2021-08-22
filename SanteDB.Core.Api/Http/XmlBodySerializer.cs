/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Serialization;
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
        // Fault serializer
        private static XmlSerializer m_faultSerializer = XmlModelSerializerFactory.Current.CreateSerializer(Type.GetType("SanteDB.Rest.Common.Fault.RestServiceFault, SanteDB.Rest.Common"));

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
            this.m_serializer = XmlModelSerializerFactory.Current.CreateSerializer(type, extraTypes);
        }

        /// <summary>
        /// Gets the serializer
        /// </summary>
        public object Serializer => this.m_serializer;

        #region IBodySerializer implementation

        /// <summary>
        /// Serialize the object
        /// </summary>
        public void Serialize(System.IO.Stream s, object o)
        {
            if (o.GetType() == this.m_type)
            {
                if(this.m_serializer == null)
                    this.m_serializer = XmlModelSerializerFactory.Current.CreateSerializer(this.m_type);

                this.m_serializer.Serialize(s, o);
            }
            else // Slower
            {
                XmlSerializer xsz = XmlModelSerializerFactory.Current.CreateSerializer(o.GetType(), (o as Bundle)?.Item.Select(i => i.GetType()).Distinct().ToArray() ?? new Type[0]);
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
                if (this.m_serializer?.CanDeserialize(bodyReader) == true)
                    serializer = this.m_serializer;

                else if (bodyReader.LocalName == "RestServiceFault" &&
                   bodyReader.NamespaceURI == "http://santedb.org/fault")
                    serializer = m_faultSerializer;
                else
                {
                    serializer = XmlModelSerializerFactory.Current.GetSerializer(bodyReader);
                    if (serializer == null)
                        throw new KeyNotFoundException($"Could not determine how to de-serialize {bodyReader.NamespaceURI}#{bodyReader.Name}");
                }
                return serializer.Deserialize(bodyReader);
            }
        }

        #endregion IBodySerializer implementation
    }
}