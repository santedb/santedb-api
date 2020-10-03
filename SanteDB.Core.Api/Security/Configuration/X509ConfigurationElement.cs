/*
 * Based on OpenIZ - Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE)
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
using Newtonsoft.Json;
using SanteDB.Core.Security;
using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Configuration
{
    /// <summary>
    /// Represents a base configuration for a X509 cert
    /// </summary>
    [XmlType(nameof(X509ConfigurationElement), Namespace = "http://santedb.org/configuration")]
    public class X509ConfigurationElement
    {

        // Certificate
        private X509Certificate2 m_certificate;

        /// <summary>
        /// The find type
        /// </summary>
        [XmlAttribute("findType"), JsonProperty("findType")]
        [DisplayName("Certificate Search")]
        [Description("Identifies the algorithm to use to locate the security certificate")]
        public X509FindType FindType { get; set; }

        /// <summary>
        /// The store name
        /// </summary>
        [XmlAttribute("storeName"), JsonProperty("storeName")]
        [DisplayName("X509 Store")]
        [Description("Identifies the secure X.509 certificate store to search")]
        public StoreName StoreName { get; set; }

        /// <summary>
        /// The store location
        /// </summary>
        [XmlAttribute("storeLocation"), JsonProperty("storeLocation")]
        [DisplayName("X509 Location")]
        [Description("Identifies the location of the X.509 certificate store to load from")]
        public StoreLocation StoreLocation { get; set; }

        /// <summary>
        /// Whether the find type was provided
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Browsable(false)]
        public bool FindTypeSpecified { get; set; }

        /// <summary>
        /// Whether the store name was provided
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Browsable(false)]
        public bool StoreNameSpecified { get; set; }

        /// <summary>
        /// Whether the store location was provided
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Browsable(false)]
        public bool StoreLocationSpecified { get; set; }

        /// <summary>
        /// The find value
        /// </summary>
        [XmlAttribute("findValue"), JsonProperty("findValue")]
        [DisplayName("Certificate Identification")]
        [Description("The certificate value to look for in the secure store")]
        [ReadOnly(true)]
        public string FindValue { get; set; }

        /// <summary>
        /// Get the certificate
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Description("The X509 certificate to use")]
        [DisplayName("Certificate")]
        [Editor("SanteDB.Configuration.Editors.X509Certificate2Editor, SanteDB.Configuration, Version=1.0.0.0", "System.Drawing.Design.UITypeEditor, System.Windows.Forms")]
        public X509Certificate2 Certificate
        {
            get => this.GetCertificate();
            set
            {
                if (value == null)
                    this.FindValue = null;
                else
                    switch(this.FindType)
                    {
                        case X509FindType.FindBySubjectName:
                            this.FindValue = value.Subject;
                            break;
                        case X509FindType.FindByThumbprint:
                            this.FindValue = value.Thumbprint;
                            break;
                        case X509FindType.FindBySerialNumber:
                            this.FindValue = value.SerialNumber;
                            break;
                        default:
                            this.FindType = X509FindType.FindByThumbprint;
                            this.FindValue = value.Thumbprint;
                            this.FindTypeSpecified = true;
                            break;
                    }
            }
        }

        /// <summary>
        /// Get the certificate
        /// </summary>
        private X509Certificate2 GetCertificate()
        {
            if(this.m_certificate != null)
            {
                X509Store store = new X509Store(this.StoreName, this.StoreLocation);
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    var matches = store.Certificates.Find(this.FindType, this.FindValue, true);
                    if (matches.Count == 0)
                        throw new InvalidOperationException("Certificate not found");
                    else if (matches.Count > 1)
                        throw new InvalidOperationException("Too many matches");
                    else
                        return matches[0];
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    store.Close();
                }
            }
            return this.m_certificate;
        }
    }
}