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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Default body binder.
    /// </summary>
    public class DefaultContentTypeMapper : IContentTypeMapper
    {

        private static readonly IDictionary<String, IBodySerializer> s_serializers;

        /// <summary>
        /// Content type mapper
        /// </summary>
        static DefaultContentTypeMapper()
        {
            s_serializers = AppDomain.CurrentDomain.GetAllTypes()
                .Where(t => t.Implements(typeof(IBodySerializer)) && !t.IsAbstract && !t.IsInterface)
                .Select(t => Activator.CreateInstance(t) as IBodySerializer)
                .ToLookup(o => o.ContentType, o => o)
                .ToDictionary(o => o.Key, o => o.First());
        }

        /// <summary>
        /// Get the content type of the file
        /// </summary>
        public static string GetContentType(string filename)
        {
            string extension = Path.GetExtension(filename);
            switch (extension.Substring(1).ToLower())
            {
                case "csv":
                    return "text/csv";
                case "htm":
                case "html":
                    return "text/html";
                case "js":
                    return "application/javascript";
                case "css":
                    return "text/css";
                case "svg":
                    return "image/svg+xml";
                case "ttf":
                    return "application/x-font-ttf";
                case "eot":
                    return "application/vnd.ms-fontobject";
                case "woff":
                    return "application/font-woff";
                case "woff2":
                    return "application/font-woff2";
                case "gif":
                    return "image/gif";
                case "ico":
                    return "image/x-icon";
                case "png":
                    return "image/png";
                case "yaml":
                    return "application/x-yaml";
                default:
                    return "application/x-octet-stream";
            }
        }
        #region IBodySerializerBinder implementation

        /// <summary>
        /// Gets the body serializer based on the content type
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <returns>The serializer.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">contentType - Not supported</exception>
        public IBodySerializer GetSerializer(ContentType contentType)
        {
            if (s_serializers.TryGetValue(contentType.MediaType, out var serializer))
            {
                return serializer;
            }
            else if (contentType.MediaType.Contains("+") && s_serializers.TryGetValue(contentType.MediaType.Split('+')[0], out serializer))
            {
                return serializer;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(contentType), contentType, "Not supported");
            }

        }

        #endregion IBodySerializerBinder implementation
    }
}