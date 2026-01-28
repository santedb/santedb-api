/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Diagnostics;
using System;
using System.IO;
using System.Net.Mime;
using System.Reflection;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Form element attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FormElementAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDB.Core.Http.FormElementAttribute"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public FormElementAttribute(String name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public String Name
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Form body serializer.
    /// </summary>
    public class FormBodySerializer : IBodySerializer
    {
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(FormBodySerializer));


        /// <inheritdoc/>
        public string ContentType => "application/x-www-form-urlencoded";

        /// <summary>
        /// Gets the underlying serializer
        /// </summary>
        public object GetSerializer(Type typeHint) => null;

        #region IBodySerializer implementation

        /// <summary>
        /// Serialize the specified object
        /// </summary>
        public void Serialize(System.IO.Stream s, object o, out ContentType contentType)
        {
            contentType = new ContentType($"{this.ContentType}; charset=utf-8");
            // Get runtime properties
            bool first = true;
            using (StreamWriter sw = new StreamWriter(s, System.Text.Encoding.UTF8, 2048, true))
            {
                foreach (var pi in o.GetType().GetRuntimeProperties())
                {
                    // Use XML Attribute
                    FormElementAttribute fatt = pi.GetCustomAttribute<FormElementAttribute>();
                    if (fatt == null)
                    {
                        continue;
                    }

                    // Write
                    String value = pi.GetValue(o)?.ToString();
                    if (String.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    if (!first)
                    {
                        sw.Write("&");
                    }

                    sw.Write("{0}={1}", fatt.Name, value);
                    first = false;
                }
            }
        }

        /// <summary>
        /// De-serialize
        /// </summary>
        public object DeSerialize(Stream s, ContentType contentType, Type typeHint)
        {
            using (StreamReader sr = new StreamReader(s, System.Text.Encoding.GetEncoding(contentType.CharSet), true, 2048, true))
            {
                return sr.ReadToEnd().ParseQueryString();
            }
        }

        #endregion IBodySerializer implementation
    }
}