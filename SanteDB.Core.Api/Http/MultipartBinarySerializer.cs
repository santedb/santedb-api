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
using SanteDB.Core.i18n;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a multipart attachment
    /// </summary>
    public class MultiPartFormData
    {

        /// <summary>
        /// Create new multipart form data with the specified data
        /// </summary>
        public MultiPartFormData(String name, String data) 
        {
            this.Data = System.Text.Encoding.UTF8.GetBytes(data);
            this.Name = name;
            this.MimeType = "text/plain; encoding=utf-8";
            this.UseFormEncoding = true;
            this.IsFile = false;
        }

        /// <summary>
        /// Creates a new multipart attachment
        /// </summary>
        public MultiPartFormData(String name, byte[] data, string mimeType, string fileName, bool useFormEncoding = false)
        {
            this.Data = data;
            this.MimeType = mimeType;
            this.Name = name;
            this.UseFormEncoding = useFormEncoding;
            this.IsFile = true;
            this.FileName = fileName;
        }

        /// <summary>
        /// Gets or sets the mime type of the attachment
        /// </summary>
        public String MimeType { get; set; }

        /// <summary>
        /// Gets or sets the name of the attachment
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Represents the data in the attachment
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// When true instructs the serializer to use form data
        /// </summary>
        public bool UseFormEncoding { get; set; }

        /// <summary>
        /// True if the mime data is a file
        /// </summary>
        public bool IsFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if(this.IsFile)
            {
                return this.FileName;
            }
            else
            {
                return Encoding.UTF8.GetString(this.Data);
            }
        }
    }

    /// <summary>
    /// Mutlipart binary serializer
    /// </summary>
    public class MultipartBinarySerializer : IBodySerializer
    {
        /// <summary>
        /// Gets the underlying serializer
        /// </summary>
        public object GetSerializer(Type typeHint) => null;

        /// <summary>
        /// Content-type
        /// </summary>
        public string ContentType => "multipart/form-data";

        /// <summary>
        /// De-serialize
        /// </summary>
        public object DeSerialize(Stream s, ContentType contentType, Type typeHint)
        {
            // TODO: Implement this
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serialize
        /// </summary>
        public void Serialize(Stream s, object o, out ContentType contentType)
        {
            // Get the boundary
            contentType = new ContentType(this.ContentType);
            var boundary = Guid.NewGuid().ToString("N");
            contentType.Parameters.Add("boundary", boundary); 
            
            // Boundary writer
            var attachmentList = o as IList<MultiPartFormData>;
            if (attachmentList == null)
            {
                throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(IList<MultiPartFormData>), o.GetType()));
            }

            using (StreamWriter sw = new StreamWriter(s))
            {
                sw.NewLine = "\r\n";
                foreach (var mimeInfo in attachmentList)
                {
                    sw.WriteLine("--{0}", boundary);

                    if (mimeInfo.UseFormEncoding)
                    {
                        if (mimeInfo.IsFile)
                        {
                            sw.WriteLine("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", mimeInfo.Name, mimeInfo.FileName);
                        }
                        else
                        {
                            sw.WriteLine("Content-Disposition: form-data; name=\"{0}\"", mimeInfo.Name);

                        }
                    }
                    else
                    {
                        sw.WriteLine("Content-Disposition: attachment; filename=\"{0}\"", mimeInfo.FileName);
                    }

                    sw.WriteLine("Content-Type: {0}", mimeInfo.MimeType);
                    sw.WriteLine();
                    sw.Flush();
                    using (MemoryStream ms = new MemoryStream(mimeInfo.Data))
                    {
                        ms.CopyTo(s);
                    }

                    sw.WriteLine();
                }
                sw.WriteLine("--{0}--", boundary);
                sw.Flush();
            }

        }
    }
}