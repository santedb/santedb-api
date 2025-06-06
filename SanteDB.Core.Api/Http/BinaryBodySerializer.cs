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
using System.IO;
using System.Net.Mime;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Binary body serializer
    /// </summary>
    public class BinaryBodySerializer : IBodySerializer
    {
        /// <summary>
        /// Gets the serializer for this body serializer
        /// </summary>
        public object GetSerializer(Type typeHint) => null;

        /// <inheritdoc/>
        public string ContentType => "application/octet-stream";

        /// <summary>
        /// De-serialize to the desired type
        /// </summary>
        public object DeSerialize(Stream s, ContentType contentType, Type typeHint)
        {
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                ms.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serialize
        /// </summary>
        public void Serialize(Stream s, object o, out ContentType contentType)
        {
            contentType = new ContentType(this.ContentType);
            if (o is byte[])
            {
                using (var ms = new MemoryStream((byte[])o))
                {
                    ms.CopyTo(s);
                }
            }
            else if (o is Stream)
            {
                (o as Stream).CopyTo(s);
            }
            else
            {
                throw new NotSupportedException("Object must be byte array");
            }
        }
    }
}