/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using Newtonsoft.Json;
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
        /// Create from X509 certificate
        /// </summary>
        public X509ConfigurationElement(X509Certificate2 certificate)
        {
            this.m_certificate = certificate;
            this.FindType = X509FindType.FindByThumbprint;
            this.StoreLocation = StoreLocation.LocalMachine;
            this.StoreName = StoreName.My;
            this.FindTypeSpecified = this.StoreLocationSpecified = true;
        }

        /// <summary>
        /// Initialize certificate settings
        /// </summary>
        public X509ConfigurationElement(StoreLocation storeLocation, StoreName storeName, X509FindType findType, String findValue)
        {
            this.StoreName = storeName;
            this.StoreLocation = storeLocation;
            this.FindType = findType;
            this.FindValue = findValue;
        }

        /// <summary>
        /// Initialize certificate settings
        /// </summary>
        public X509ConfigurationElement()
        {
            this.FindType = X509FindType.FindByThumbprint;
            this.StoreLocation = StoreLocation.LocalMachine;
            this.StoreName = StoreName.My;
            this.FindTypeSpecified = this.StoreLocationSpecified = true;
        }

        /// <summary>
        /// Validation only?
        /// </summary>
        [XmlIgnore, JsonProperty]
        [Browsable(false)]
        public bool ValidationOnly { get; set; }

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
        [Editor("SanteDB.Configuration.Editors.X509Certificate2Editor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public X509Certificate2 Certificate
        {
            get => this.GetCertificate();
            set
            {
                if (value == null)
                    this.FindValue = null;
                else
                    switch (this.FindType)
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
            if (this.m_certificate == null)
            {
                try
                {
                    X509Store store = new X509Store(this.StoreName, this.StoreLocation);
                    try
                    {
                        store.Open(OpenFlags.ReadOnly);
                        var matches = store.Certificates.Find(this.FindType, this.FindValue, false);
                        if (matches.Count == 0)
                            throw new InvalidOperationException("Certificate not found");
                        else if (matches.Count > 1)
                            throw new InvalidOperationException("Too many matches");
                        else
                        {
                            this.m_certificate = matches[0];
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    finally
                    {
                        store.Close();
                    }
                }
                catch
                {
                    return null;
                }
            }
            return this.m_certificate;
        }

        /// <summary>
        /// Certificate binding
        /// </summary>
        public override string ToString() => this.Certificate?.ToString();
    }
}