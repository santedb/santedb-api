/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-28
 */
using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Data
{
    /// <summary>
    /// Data configuration section
    /// </summary>
    [XmlType(nameof(DataConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Initializes a new instance of the data configuration section
        /// </summary>
        public DataConfigurationSection()
        {
            this.ConnectionString = new List<ConnectionString>();
        }

        /// <summary>
        /// Gets or sets connection strings
        /// </summary>
        /// <value>My property.</value>
        [XmlElement("connectionString"), JsonIgnore]
        public List<ConnectionString> ConnectionString
        {
            get;
            set;
        }

    }


    /// <summary>
    /// Represents a single connection string
    /// </summary>
    [XmlType(nameof(ConnectionString), Namespace = "http://santedb.org/configuration")]
    public class ConnectionString
    {

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlAttribute("name")]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        [XmlAttribute("value")]
        public String Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the provider invariant
        /// </summary>
        [XmlAttribute("provider")]
        public String Provider { get; set; }

        /// <summary>
        /// Get the component
        /// </summary>
        public String GetComponent(String component)
        {
            var values = this.Value.Split(';').ToDictionary(o => o.Split('=')[0].Trim(), o => o.Split('=')[1].Trim());
            String retVal = null;
            values.TryGetValue(component, out retVal);
            return retVal;
        }

        /// <summary>
        /// Clones a readonly copy of this connection string
        /// </summary>
        public ConnectionString Clone()
        {
            return (ConnectionString)this.MemberwiseClone();
        }

        /// <summary>
        /// Represent the connection string a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Value;
        }
    }

}

