/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// SanteDB Base configuration
    /// </summary>
    [XmlType(nameof(SanteDBBaseConfiguration), Namespace = "http://santedb.org/configuration")]
    [XmlRoot(nameof(SanteDBConfiguration), Namespace = "http://santedb.org/configuration")]
    public class SanteDBBaseConfiguration
    {
        /// <summary>
        /// Gets the list of section types in this configuration
        /// </summary>
        [XmlArray("sections"), XmlArrayItem("add")]
        public List<TypeReferenceConfiguration> SectionTypes { get; set; }

        /// <summary>
        /// Base configuration
        /// </summary>
        public SanteDBBaseConfiguration()
        {
            this.SectionTypes = new List<TypeReferenceConfiguration>();
        }
    }

    /// <summary>
    /// Configuration table object
    /// </summary>
    [XmlRoot(nameof(SanteDBConfiguration), Namespace = "http://santedb.org/configuration")]
    [XmlType(nameof(SanteDBConfiguration), Namespace = "http://santedb.org/configuration")]
    public sealed class SanteDBConfiguration : SanteDBBaseConfiguration
    {
        // Serializer
        private static XmlSerializer s_baseSerializer = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration));
        private static XmlSerializer s_serializer;

        /// <summary>
        /// SanteDB configuration
        /// </summary>
        public SanteDBConfiguration()
        {
            this.Sections = new List<Object>();
            this.Version = typeof(SanteDBConfiguration).GetTypeInfo().Assembly.GetName().Version.ToString();
        }
        
        /// <summary>
        /// Gets or sets the version of the configuration
        /// </summary>
        /// <value>The version.</value>
        [XmlAttribute("version")]
        public String Version
        {
            get { return typeof(SanteDBConfiguration).GetTypeInfo().Assembly.GetName().Version.ToString(); }
            set
            {

                Version v = new Version(value),
                    myVersion = typeof(SanteDBConfiguration).GetTypeInfo().Assembly.GetName().Version;
                if (v.Major > myVersion.Major)
                    throw new ConfigurationException(String.Format("Configuration file version {0} is newer than SanteDB version {1}", v, myVersion));
            }
        }

        /// <summary>
        /// Load the specified dataStream.
        /// </summary>
        /// <param name="dataStream">Data stream.</param>
        public static SanteDBConfiguration Load(Stream dataStream)
        {
            var configStream = dataStream;
            if (!configStream.CanSeek)
            {
                configStream = new MemoryStream();
                dataStream.CopyTo(configStream);
                configStream.Seek(0, SeekOrigin.Begin);
            }

            // Load the base types
            var tbaseConfig = s_baseSerializer.Deserialize(configStream) as SanteDBBaseConfiguration;
            configStream.Seek(0, SeekOrigin.Begin);

            if (s_serializer == null)
                s_serializer = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), tbaseConfig.SectionTypes.Select(o => o.Type).Where(o => o != null).ToArray());

            return s_serializer.Deserialize(configStream) as SanteDBConfiguration;
        }

        /// <summary>
        /// Save the configuration to the specified data stream
        /// </summary>
        /// <param name="dataStream">Data stream.</param>
        public void Save(Stream dataStream)
        {
            this.SectionTypes = this.Sections.Select(o => new TypeReferenceConfiguration(o.GetType())).ToList();
            var namespaces = this.Sections.Select(o => o.GetType().GetTypeInfo().GetCustomAttribute<XmlTypeAttribute>()?.Namespace).OfType<String>().Where(o=>o.StartsWith("http://santedb.org/configuration/")).Distinct().Select(o=>new XmlQualifiedName(o.Replace("http://santedb.org/configuration/", ""), o)).ToArray();
            XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(namespaces);
            xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            if (s_serializer == null)
                s_serializer = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), this.SectionTypes.Select(o => o.Type).Where(o => o != null).ToArray());

            s_serializer.Serialize(dataStream, this, xmlns);
        }

        /// <summary>
        /// Configuration sections
        /// </summary>
        /// <value>The sections.</value>
        [XmlElement("section")]
        public List<Object> Sections
        {
            get;
            set;
        }

        /// <summary>
        /// Get the specified section
        /// </summary>
        /// <returns>The section.</returns>
        public T GetSection<T>() 
        {
            return (T)this.GetSection(typeof(T));
        }

        /// <summary>
        /// Gets the section of specified type.
        /// </summary>
        /// <returns>The section.</returns>
        /// <param name="t">T.</param>
        public object GetSection(Type t)
        {
            return this.Sections.Find(o => t.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo()));
        }

        /// <summary>
        /// Add the specified section
        /// </summary>
        public void AddSection<T>(T section)
        {
            if (!this.SectionTypes.Any(o => o.Type == typeof(T)))
                this.SectionTypes.Add(new TypeReferenceConfiguration(typeof(T)));
            this.Sections.Add(section);
        }

        /// <summary>
        /// Remove the specified section
        /// </summary>
        /// <typeparam name="T">Removes the specified section</typeparam>
        public void RemoveSection<T>()
        {
            this.Sections.RemoveAll(o => o is T);
        }
    }
}

