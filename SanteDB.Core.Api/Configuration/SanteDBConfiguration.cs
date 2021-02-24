/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
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

        /// <summary>
        /// SanteDB configuration
        /// </summary>
        public SanteDBConfiguration()
        {
            this.Sections = new List<Object>();
            this.Version = typeof(SanteDBConfiguration).Assembly.GetName().Version.ToString();
        }
        
        /// <summary>
        /// Gets or sets the version of the configuration
        /// </summary>
        /// <value>The version.</value>
        [XmlAttribute("version")]
        public String Version
        {
            get { return typeof(SanteDBConfiguration).Assembly.GetName().Version.ToString(); }
            set
            {

                Version v = new Version(value),
                    myVersion = typeof(SanteDBConfiguration).Assembly.GetName().Version;
                if (v.Major > myVersion.Major)
                    throw new ConfigurationException(String.Format("Configuration file version {0} is newer than SanteDB version {1}", v, myVersion), this);
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
            var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), tbaseConfig.SectionTypes.Select(o => o.Type).Where(o => o != null).ToArray());

            var retVal = xsz.Deserialize(configStream) as SanteDBConfiguration;
            if (retVal.Sections.Any(o => o is XmlNode[]))
            {
                string allowedSections = String.Join(";", tbaseConfig.SectionTypes.Select(o => $"{o.Type?.GetCustomAttribute<XmlTypeAttribute>().TypeName} (in {o.TypeXml})"));
                throw new ConfigurationException($"Could not understand configuration sections: {String.Join(",", retVal.Sections.OfType<XmlNode[]>().Select(o => o.First().Value))} allowed sections {allowedSections}", retVal);
            }

            if(retVal.Includes != null)
                foreach(var incl in retVal.Includes)
                {
                    string fileName = incl;
                    if (!Path.IsPathRooted(fileName))
                        fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);

                    if (File.Exists(fileName))
                        using (var fs = File.OpenRead(fileName))
                        {
                            var inclData = SanteDBConfiguration.Load(fs);
                            retVal.Sections.AddRange(inclData.Sections);
                        }
                    else
                        throw new ConfigurationException($"Include {fileName} was not found", retVal);                }
            

            return retVal;
        }

        /// <summary>
        /// Save the configuration to the specified data stream
        /// </summary>
        /// <param name="dataStream">Data stream.</param>
        public void Save(Stream dataStream)
        {
            this.SectionTypes = this.Sections.Select(o => new TypeReferenceConfiguration(o.GetType())).ToList();
            var namespaces = this.Sections.Select(o => o.GetType().GetCustomAttribute<XmlTypeAttribute>()?.Namespace).OfType<String>().Where(o=>o.StartsWith("http://santedb.org/configuration/")).Distinct().Select(o=>new XmlQualifiedName(o.Replace("http://santedb.org/configuration/", ""), o)).ToArray();
            XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(namespaces);
            xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), this.SectionTypes.Select(o => o.Type).Where(o => o != null).ToArray());
            xsz.Serialize(dataStream, this, xmlns);
        }

        /// <summary>
        /// Includes
        /// </summary>
        [XmlElement("include")]
        public List<String> Includes { get; set; }

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
            return this.Sections.Find(o => t.IsAssignableFrom(o.GetType()));
        }

        /// <summary>
        /// Add the specified section
        /// </summary>
        public void AddSection<T>(T section)
        {
            if (!this.SectionTypes.Any(o => o.Type == typeof(T)))
            {
                this.SectionTypes.Add(new TypeReferenceConfiguration(typeof(T)));
            }
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

