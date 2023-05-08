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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Security.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
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
        private static XmlSerializer s_baseSerializer = new XmlSerializer(typeof(SanteDBConfiguration));

        /// <summary>
        /// Initialization vector of the configuration section
        /// </summary>
        [XmlAttribute("eci")]
        public byte[] EncryptionMetadata { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="X509Certificate2"/> which protects any instance of
        /// <see cref="IEncryptedConfigurationSection"/> instances
        /// </summary>
        [XmlElement("protectedSectionKey")]
        public X509ConfigurationElement ProtectedSectionKey { get; set; }

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
                {
                    throw new ConfigurationException(String.Format("Configuration file version {0} is newer than SanteDB version {1}", v, myVersion), this);
                }
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
            tbaseConfig.SectionTypes.Add(new TypeReferenceConfiguration(typeof(SanteDBProtectedConfigurationSectionWrapper)));
            var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), tbaseConfig.SectionTypes.Select(o => o.Type).Where(o => o != null).ToArray());

            var retVal = xsz.Deserialize(configStream) as SanteDBConfiguration;
            if (retVal.Sections.Any(o => o is XmlNode[]))
            {
                string allowedSections = String.Join(";", tbaseConfig.SectionTypes.Select(o => $"{o.Type?.GetCustomAttribute<XmlTypeAttribute>()?.TypeName} (in {o.TypeXml})"));
                throw new ConfigurationException($"Could not understand configuration sections: {String.Join(",", retVal.Sections.OfType<XmlNode[]>().Select(o => o.First().Value))} allowed sections {allowedSections}", retVal);
            }

            if (retVal.ProtectedSectionKey != null && retVal.EncryptionMetadata != null)
            {

                using (var crypto = retVal.ProtectedSectionKey.Certificate.GetRSAPrivateKey())
                {
                    var aesKey = crypto.Decrypt(retVal.EncryptionMetadata, RSAEncryptionPadding.Pkcs1);
                    retVal.Sections = retVal.Sections.Select(o =>
                    {
                        if (o is SanteDBProtectedConfigurationSectionWrapper w)
                        {
                            return w.Decrypt(aesKey);
                        }
                        else
                        {
                            return o;
                        }
                    }).ToList();
                }
            }

            if (retVal.Includes != null)
            {
                foreach (var incl in retVal.Includes)
                {
                    string fileName = incl.Replace('\\', Path.DirectorySeparatorChar);
                    if (!Path.IsPathRooted(fileName))
                    {
                        fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
                    }

                    if (File.Exists(fileName))
                    {
                        using (var fs = File.OpenRead(fileName))
                        {
                            var inclData = SanteDBConfiguration.Load(fs);
                            retVal.Sections.AddRange(inclData.Sections);
                        }
                    }

                    else
                    {
                        throw new ConfigurationException($"Include {fileName} was not found", retVal);
                    }
                }
            }

            // Validate the configuration 
            var errors = retVal.Sections.OfType<IValidatableConfigurationSection>().SelectMany(o => o.Validate()).Where(d => d.Priority == DetectedIssuePriorityType.Error);
            if (errors.Any())
            {
                throw new ConfigurationException($"Error validating configuration: {String.Join("\r\n", errors.Select(o => o.Text))}", retVal);
            }

            return retVal;
        }

        /// <summary>
        /// Validate the specified configuration stream
        /// </summary>
        /// <param name="dataStream">The stream which contains the configuration data</param>
        /// <returns>The list of configuration issues with the stream</returns>
        public static IEnumerable<DetectedIssue> Validate(Stream dataStream)
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
            // Validate the section types
            foreach (var sct in tbaseConfig.SectionTypes)
            {
                if (!sct.IsValid())
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "section", $"Section {sct.TypeXml} is not valid", Guid.Empty);
                }
            }

            configStream.Seek(0, SeekOrigin.Begin);
            var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), tbaseConfig.SectionTypes.Where(o => o.IsValid()).Select(o => o.Type).ToArray());

            // Re-load
            var config = xsz.Deserialize(configStream) as SanteDBConfiguration;

            foreach (var xn in config.Sections.OfType<XmlNode[]>().Select(o => o.First().Value))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "unknown", $"Section {xn} is unknown", Guid.Empty);
            }

            if (config.Includes != null)
            {
                foreach (var inc in config.Includes)
                {
                    if (!File.Exists(inc))
                    {
                        yield return new DetectedIssue(DetectedIssuePriorityType.Error, "include", $"Include {inc} is missing", Guid.Empty);
                    }
                    else
                    {
                        using (var fs = File.OpenRead(inc))
                        {
                            foreach (var itm in SanteDBConfiguration.Validate(fs))
                            {
                                yield return new DetectedIssue(itm.Priority, $"include.{itm.Id}", $"({inc}): {itm.Text}", Guid.Empty);
                            }
                        }
                    }
                }
            }

            // Validate the main section
            foreach (var sc in config.Sections.OfType<IValidatableConfigurationSection>())
            {
                foreach (var itm in sc.Validate())
                {
                    yield return new DetectedIssue(itm.Priority, $"section.{itm.Id}", $"(#{sc.GetType().Name}) - {itm.Text}", Guid.Empty);
                }
            }


        }

        /// <summary>
        /// Save the configuration to the specified data stream
        /// </summary>
        /// <param name="dataStream">Data stream.</param>
        public void Save(Stream dataStream)
        {
            this.SectionTypes = this.Sections?.OfType<IConfigurationSection>().Select(o => new TypeReferenceConfiguration(o.GetType())).ToList();
            this.SectionTypes.Add(new TypeReferenceConfiguration(typeof(SanteDBProtectedConfigurationSectionWrapper)));

            var namespaces = this.Sections.OfType<IConfigurationSection>().Select(o => o.GetType().GetCustomAttribute<XmlTypeAttribute>()?.Namespace).OfType<String>().Where(o => o.StartsWith("http://santedb.org/configuration/")).Distinct().Select(o => new XmlQualifiedName(o.Replace("http://santedb.org/configuration/", ""), o)).ToArray();
            XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(namespaces);
            xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(SanteDBConfiguration), this.SectionTypes.OfType<TypeReferenceConfiguration>().Select(o => o.Type).Where(o => o != null).ToArray());

            // Any protected sections we need to encrypt using the configured key
            if (this.ProtectedSectionKey != null)
            {
                var cryptoConfig = this.MemberwiseClone() as SanteDBConfiguration;
                
                if(this.ProtectedSectionKey.Certificate == null)
                {
                    throw new InvalidOperationException("Certificate must be specified for encryption");
                }
                else if(!this.ProtectedSectionKey.Certificate.HasPrivateKey)
                {
                    throw new InvalidOperationException($"Certificate {this.ProtectedSectionKey.Certificate.Subject} must have private key for encryption");
                }

                using (var crypto = this.ProtectedSectionKey.Certificate.GetRSAPrivateKey())
                {
                    byte[] aesKey = Guid.NewGuid().ToByteArray();
                    cryptoConfig.ProtectedSectionKey = this.ProtectedSectionKey;
                    cryptoConfig.EncryptionMetadata = crypto.Encrypt(aesKey, RSAEncryptionPadding.Pkcs1); // Save the encrypted secret in the config file
                    cryptoConfig.Sections = cryptoConfig.Sections.Select(o =>
                    {
                        if (o is IEncryptedConfigurationSection)
                        {
                            return new SanteDBProtectedConfigurationSectionWrapper(aesKey, o as IConfigurationSection);
                        }
                        else
                        {
                            return o;
                        }
                    }).ToList();
                    xsz.Serialize(dataStream, cryptoConfig, xmlns);

                }
            }
            else
            {
                xsz.Serialize(dataStream, this, xmlns);
            }
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
        /// Remove the specified section
        /// </summary>
        public void RemoveSection(Type type)
        {
            this.Sections.RemoveAll(o => o.GetType() == type);
        }

        /// <summary>
        /// Gets the section of specified type.
        /// </summary>
        /// <returns>The section.</returns>
        /// <param name="t">T.</param>
        public object GetSection(Type t)
        {
            return this.Sections.OfType<IConfigurationSection>().FirstOrDefault(o => t.IsAssignableFrom(o.GetType()));
        }

        /// <summary>
        /// Add the specified section
        /// </summary>
        public T AddSection<T>(T section)
        {
            if (section == null)
            {
                throw new InvalidOperationException("Cannot add a null section");
            }
            if (!this.SectionTypes.Any(o => o.Type == typeof(T)))
            {
                this.SectionTypes.Add(new TypeReferenceConfiguration(typeof(T)));
            }
            this.Sections.Add(section);
            return section;
        }

        /// <summary>
        /// Remove the specified section
        /// </summary>
        /// <typeparam name="T">Removes the specified section</typeparam>
        public void RemoveSection<T>()
        {
            this.Sections.RemoveAll(o => o is T);
        }

        /// <summary>
        /// Protected configuration section
        /// </summary>
        [XmlType(nameof(SanteDBProtectedConfigurationSectionWrapper), Namespace = "http://santedb.org/configuration")]
        public class SanteDBProtectedConfigurationSectionWrapper : IConfigurationSection
        {
            /// <summary>
            /// Creates a new protected section wrapper
            /// </summary>
            public SanteDBProtectedConfigurationSectionWrapper()
            {
            }

            /// <summary>
            /// Create a protected wrapper
            /// </summary>
            public SanteDBProtectedConfigurationSectionWrapper(byte[] aesKey, IConfigurationSection wrapped)
            {
                this.WrappedType = wrapped.GetType().AssemblyQualifiedName;
                var aes = AesCryptoServiceProvider.Create();
                aes.GenerateIV();
                this.Iv = aes.IV;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(aesKey, aes.IV), CryptoStreamMode.Write))
                    {
                        new XmlSerializer(wrapped.GetType()).Serialize(cs, wrapped);
                    }
                    this.ProtectedContents = ms.ToArray();
                }
            }

            /// <summary>
            /// Decrypt the protected section
            /// </summary>
            public IConfigurationSection Decrypt(byte[] aesKey)
            {
                var aes = AesCryptoServiceProvider.Create();
                using (var ms = new MemoryStream(this.ProtectedContents))
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(aesKey, this.Iv), CryptoStreamMode.Read))
                    {
                        return new XmlSerializer(Type.GetType(this.WrappedType)).Deserialize(cs) as IConfigurationSection;
                    }
                }
            }

            /// <summary>
            /// The original type
            /// </summary>
            [XmlAttribute("t")]
            public String WrappedType { get; set; }

            /// <summary>
            /// The protected contents
            /// </summary>
            [XmlElement("c")]
            public byte[] ProtectedContents { get; set; }

            /// <summary>
            /// Initialization vector
            /// </summary>
            [XmlElement("i")]
            public byte[] Iv { get; set; }
        }
    }
}