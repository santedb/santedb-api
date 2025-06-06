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
    /// Defines behavior of a content/type mapper
    /// </summary>
    public interface IBodySerializer
    {

        /// <summary>
        /// Get the content type that this serializes
        /// </summary>
        String ContentType { get; }

        /// <summary>
        /// Gets the serializer for this body serializer
        /// </summary>
        object GetSerializer(Type typeHint);

        /// <summary>
        /// Serialize the specified object
        /// </summary>
        void Serialize(Stream requestOrResponseStream, Object objectToSerialize, out ContentType contentType);

        /// <summary>
        /// Serialize the reply stream
        /// </summary>
        Object DeSerialize(Stream requestOrResponseStream, ContentType contentType, Type typeHint);
    }

    /// <summary>
    /// Defines a class that binds a series of serializers to content/types
    /// </summary>
    public interface IContentTypeMapper
    {
        /// <summary>
        /// Gets the body serializer based on the content type
        /// </summary>
        IBodySerializer GetSerializer(ContentType contentType);

    }
}