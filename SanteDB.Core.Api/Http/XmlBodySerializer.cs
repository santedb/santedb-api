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
using SanteDB.Core.Model.Serialization;
using System;
using System.IO;
using System.Net.Mime;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a body serializer that uses XmlSerializer
    /// </summary>
    public class XmlBodySerializer : IBodySerializer
    {
        // Fault serializer
        private static XmlSerializer m_faultSerializer = XmlModelSerializerFactory.Current.CreateSerializer(Type.GetType("SanteDB.Rest.Common.Fault.RestServiceFault, SanteDB.Rest.Common"));

        /// <summary>
        /// Content type
        /// </summary>
        public string ContentType => "application/xml";

        /// <summary>
        /// Gets the serializer
        /// </summary>
        public object GetSerializer(Type typeHint) => this.GetSerializerInternal(typeHint);

        /// <summary>
        /// Get serializer
        /// </summary>
        private XmlSerializer GetSerializerInternal(Type typeHint) => XmlModelSerializerFactory.Current.CreateSerializer(typeHint);

        #region IBodySerializer implementation

        /// <summary>
        /// Serialize the object
        /// </summary>
        public void Serialize(System.IO.Stream s, object o, out ContentType contentType)
        {
            contentType = new ContentType($"{this.ContentType}; charset=utf-8");
            using (var sw = new StreamWriter(s, System.Text.Encoding.UTF8))
            using (var xw = XmlWriter.Create(sw))
            {
                this.GetSerializerInternal(o.GetType()).Serialize(xw, o);
            }
        }

        /// <summary>
        /// Serialize the reply stream
        /// </summary>
        public object DeSerialize(System.IO.Stream s, ContentType contentType, Type typeHint)
        {
            using (var sr = new StreamReader(s, System.Text.Encoding.GetEncoding(contentType.CharSet ?? "utf-8")))
            using (var xr = XmlReader.Create(sr))
            {
                while (xr.NodeType != XmlNodeType.Element)
                {
                    xr.Read();
                }

                var serializer = this.GetSerializerInternal(typeHint);
                if (serializer.CanDeserialize(xr))
                {
                    return serializer.Deserialize(xr);
                }
                else if (xr.LocalName == "RestServiceFault" &&
                   xr.NamespaceURI == "http://santedb.org/fault")
                {
                    return m_faultSerializer.Deserialize(xr);
                }
                else
                {
                    return XmlModelSerializerFactory.Current.GetSerializer(xr)?.Deserialize(xr)
                        ?? throw new InvalidOperationException($"{xr.LocalName} has no serializer!");
                }
            }
        }

        #endregion IBodySerializer implementation
    }
}