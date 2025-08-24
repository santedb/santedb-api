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
using SanteDB.Core.ViewModel.Json;
using SanteDB.Core.Http;
using SanteDB.Core.Model;
using System;
using System.IO;
using System.Net.Mime;

namespace SanteDB.Core.ViewModel
{
    /// <summary>
    /// View model body serializer
    /// </summary>
    public class ViewModelBodySerializer : JsonBodySerializer
    {
        /// <inheritdoc/>
        public override string ContentType => SanteDBExtendedMimeTypes.JsonViewModel; //"application/json+sdb-viewmodel";

        /// <inheritdoc/>
        public override object DeSerialize(Stream requestOrResponseStream, ContentType contentType, Type typeHint)
        {
            using (var sr = new StreamReader(requestOrResponseStream, System.Text.Encoding.GetEncoding(contentType.CharSet ?? "utf-8"), true, 2048, true))
            {
                var serializer = new JsonViewModelSerializer();
                return serializer.DeSerialize(sr, typeHint);
            }
        }

        /// <inheritdoc/>
        public override object GetSerializer(Type typeHint) => new JsonViewModelSerializer();

        /// <inheritdoc/>
        public override void Serialize(Stream requestOrResponseStream, object objectToSerialize, out ContentType contentType)
        {
            using (var sw = new StreamWriter(requestOrResponseStream, System.Text.Encoding.UTF8, 2048, true))
            {
                if (objectToSerialize is IdentifiedData identifiedData)
                {
                    var serializer = new JsonViewModelSerializer();
                    contentType = new ContentType($"{this.ContentType}; charset=utf-8");
                    serializer.Serialize(sw, identifiedData);
                }
                else
                {
                    base.Serialize(requestOrResponseStream, objectToSerialize, out contentType);
                }
            }
        }
    }
}
