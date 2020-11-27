﻿/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2020-5-1
 */
using System;
using System.IO;

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
        public object Serializer => null;

        /// <summary>
        /// De-serialize to the desired type
        /// </summary>
        public object DeSerialize(Stream s)
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
        public void Serialize(Stream s, object o)
        {
            if (o is byte[])
            {
                using (var ms = new MemoryStream((byte[])o))
                    ms.CopyTo(s);
            }
            else if (o is Stream)
                (o as Stream).CopyTo(s);
            else
                throw new NotSupportedException("Object must be byte array");
        }
    }
}