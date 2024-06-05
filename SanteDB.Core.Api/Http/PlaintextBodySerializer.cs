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
using System;
using System.IO;
using System.Net.Mime;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// A body serializer that serializes and deserializes strings into a plaintext body with the content type <c>text/plain</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="DeSerialize(Stream, ContentType, Type)"/> will always return a string. <br />
    /// <see cref="Serialize(Stream, object, out ContentType)"/> will call <see cref="string.ToString()"/> on the object passed for serialization.
    /// </remarks>
    public class PlaintextSerializer : IBodySerializer
    {
        /// <inheritdoc/>
        public string ContentType => "text/plain";

        /// <inheritdoc/>
        public object DeSerialize(Stream requestOrResponseStream, ContentType contentType, Type typeHint)
        {
            using (var sr = new StreamReader(requestOrResponseStream, System.Text.Encoding.GetEncoding(contentType.CharSet ?? "utf-8")))
            {
                return sr.ReadToEnd();
            }
        }

        /// <inheritdoc/>
        public object GetSerializer(Type typeHint) => null;

        /// <inheritdoc/>
        public void Serialize(Stream requestOrResponseStream, object objectToSerialize, out ContentType contentType)
        {
            using (var sw = new StreamWriter(requestOrResponseStream))
            {
                sw.Write(objectToSerialize);
                contentType = new ContentType($"text/plain; charset={sw.Encoding.WebName}");
            }
        }
    }
}
