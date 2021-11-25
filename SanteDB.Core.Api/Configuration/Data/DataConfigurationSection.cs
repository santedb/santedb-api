/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

#pragma warning disable  CS1587
/// <summary>
/// The SanteDB.Core.Configuration.Data namespace contains the configuration sections which control the SanteDB database
/// providers, ORM functions and connectivity to services.
/// </summary>
#pragma warning restore  CS1587

namespace SanteDB.Core.Configuration.Data
{
    /// <summary>
    /// Configuration section where data connection strings (which dictate how to connect to the primary data store)
    /// are defined.
    /// </summary>
    /// <remarks>
    /// <para>SanteDB plugins often require access to one or more databases. This functionality is common across the iCDR and dCDR
    /// services. The rationale for separating this value from the <c>app.config</c> file is portablility of the configuration
    /// between Xamarin, .NET Core, .NET Framework, etc. Additionally, the serialization of this class allows for quick
    /// access to change and save the value contained therein (opposed to the much more obtuse ConfigurationSectionHandler pattern).</para>
    /// </remarks>
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
        /// Gets or sets the collection of connection strings defined in the SanteDB CDR
        /// </summary>
        /// <remarks>Connection strings are comprised of three parts: a name, which identifies the connection string (example: production, main, etc.); the
        /// value which defines the connection parameters (example: server address, user password, etc.); and, the provider (example: PostgreSQL, Firebird, etc.)</remarks>
        [XmlArray("connectionStrings"), XmlArrayItem("add"), JsonIgnore]
        public List<ConnectionString> ConnectionString
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A single connection string in the SanteDB CDR configuration file
    /// </summary>
    [XmlType(nameof(ConnectionString), Namespace = "http://santedb.org/configuration")]
    public class ConnectionString
    {
        /// <summary>
        /// Connection string
        /// </summary>
        public ConnectionString()
        {
            this.Value = String.Empty;
        }

        /// <summary>
        /// Creates a new instance of the connection string for the specified <paramref name="providerName"/>
        /// </summary>
        /// <param name="connectionString">The connection string as defined by the <paramref name="providerName"/> API. Connection strings are commonly key=value pairs separated by semi-colons (example: <c>server=localhost;user=username;password=password</c>)</param>
        /// <param name="providerName">The name of the registered provider (see: OrmConfigurationSection for defining provider invariant names</param>
        public ConnectionString(string providerName, string connectionString)
        {
            this.Provider = providerName;
            this.Value = connectionString;
        }

        /// <summary>
        /// Creates a new connection string with the specified <paramref name="providerName"/>
        /// </summary>
        /// <param name="providerName">The name of the regisered provider (see: OrmConfigurationSection for defining provider invariant names)</param>
        /// <param name="values">The values for the connection string expressed as a dictionary of key-value pairs rather than a string</param>
        public ConnectionString(String providerName, IDictionary<String, Object> values)
        {
            this.Provider = providerName;
            this.Value = String.Empty;
            foreach (var itm in values)
                this.SetComponent(itm.Key, itm.Value?.ToString());
        }

        /// <summary>
        /// Gets or sets the name of the connection string
        /// </summary>
        /// <remarks>The name of the connection string provides a friendly name which the rest of the SanteDB API
        /// components to reference this connection. Common connection string names can represent things like
        /// <c>main</c> for the main database, or <c>audit</c> for the audit database.
        /// <para>If you're using the ConfigurationTool to drive your configuration file, the name of the connection string will be a random,
        /// unique identifier.</para></remarks>
        [XmlAttribute("name")]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection string value in key=value;key=value format.
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
        /// <remarks>
        /// The provider invariant is the unique identifier for the database ORM provider. The ORM layer uses this information to construct an appropriate client
        /// to connect and interact with the database. The provider name is registered in the ORM configuration section , and provides a more friendly manner of accessing
        /// the provider registration
        /// </remarks>
        [XmlAttribute("provider")]
        public String Provider { get; set; }

        /// <summary>
        /// Get an individual component part of the connection string
        /// </summary>
        /// <param name="component">The name of the component</param>
        /// <returns>The value of the component</returns>
        /// <example lang="C#">
        /// <code language="cs">
        /// <![CDATA[
        /// var cstr = new ConnectionString("npgsql", "server=foo;database=bar;user=yosemite;password=Sam");
        /// Console.Write(cstr.GetComponent("server"));
        /// Console.Write(cstr.GetComponent("database"));
        /// ]]>
        /// </code>
        /// Output:
        /// foo
        /// bar
        /// </example>
        public String GetComponent(String component)
        {
            var values = this.Value.Split(';').Where(t => t.Contains("=")).ToDictionary(o => o.Split('=')[0].Trim(), o => o.Split('=')[1].Trim());

            String retVal = null;
            values.TryGetValue(component, out retVal);
            return retVal;
        }

        /// <summary>
        /// Set the specified <paramref name="component"/> of the connection string to <paramref name="value"/> or adds it if there is no current value
        /// </summary>
        /// <param name="component">The component of the connection string to set</param>
        /// <param name="value">The value of the connection string to set</param>
        /// <example>
        /// <code language="cs">
        /// <![CDATA[
        /// var cstr = new ConnectionString("npgsql", "Server=foo;Database=bar;");
        /// cstr.SetComponent("server","foo.bar"); // Connection string is now : Server=foo.bar;Database=bar
        /// cstr.SetComponent("user","yosemite"); // Connection string is now: Server=foo.bar;Database=bar;user=yosemite
        /// ]]></code></example>
        public void SetComponent(String component, String value)
        {
            var values = this.Value.Split(';').Where(t => t.Contains("=")).ToDictionary(o => o.Split('=')[0].Trim(), o => o.Split('=')[1].Trim());
            if (values.ContainsKey(component))
            {
                if (String.IsNullOrEmpty(value))
                    values.Remove(component);
                else
                    values[component] = value;
            }
            else if (!String.IsNullOrEmpty(value))
                values.Add(component, value.ToString());
            this.Value = String.Join(";", values.Select(o => $"{o.Key}={o.Value}"));
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
        /// <returns>The string representation of the connection string</returns>
        public override string ToString()
        {
            return this.Value;
        }
    }
}