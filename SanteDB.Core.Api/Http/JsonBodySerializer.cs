/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SanteDB.Core.Model.Serialization;
using System;
using System.IO;
using System.Net.Mime;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a body serializer that uses JSON
    /// </summary>
    public class JsonBodySerializer : IBodySerializer
    {
        /// <summary>
        /// Json formatter
        /// </summary>
        public virtual string ContentType => "application/json";

        /// <summary>
        /// Gets the underlying serializer
        /// </summary>
        public virtual object GetSerializer(Type typeHint) => this.GetSerializerInternal();

        /// <summary>
        /// Get serializer for json
        /// </summary>
        private JsonSerializer GetSerializerInternal()
        {
            var retVal = new JsonSerializer()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new ModelSerializationBinder()
            };
            retVal.Converters.Add(new StringEnumConverter());
            return retVal;
        }

        #region IBodySerializer implementation

        /// <summary>
        /// Serialize
        /// </summary>
        public virtual void Serialize(System.IO.Stream s, object o, out ContentType contentType)
        {
            contentType = new ContentType($"{this.ContentType}; charset=utf-8");
            using (TextWriter tw = new StreamWriter(s, System.Text.Encoding.UTF8, 2048, true))
            using (JsonTextWriter jw = new JsonTextWriter(tw))
            {
                this.GetSerializerInternal().Serialize(jw, o);
            }
        }

        /// <summary>
        /// De-serialize the body
        /// </summary>
        public virtual object DeSerialize(System.IO.Stream s, ContentType contentType, Type typeHint)
        {
            try
            {
                using (TextReader tr = new StreamReader(s, System.Text.Encoding.GetEncoding(contentType.CharSet ?? "utf-8"), true, 2048, true))
                using (JsonTextReader jr = new JsonTextReader(tr))
                {

                    return this.GetSerializerInternal().Deserialize(jr, typeHint);
                }
            }
            catch
            {
                if (s.CanSeek)
                {
                    s.Seek(0, SeekOrigin.Begin);
                    using (TextReader tr = new StreamReader(s, System.Text.Encoding.GetEncoding(contentType.CharSet ?? "utf-8"), true, 2048, true))
                    using (JsonTextReader jr = new JsonTextReader(tr))
                    {
                        return this.GetSerializerInternal().Deserialize(jr, Type.GetType("SanteDB.Rest.Common.Fault.RestServiceFault, SanteDB.Rest.Common"));
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion IBodySerializer implementation
    }
}