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
using ExpressionEvaluator;
using Newtonsoft.Json;
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Configuration Section for configuring the retention policies
    /// </summary>
    [XmlType(nameof(DataRetentionConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataRetentionConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Creates a new retention
        /// </summary>
        public DataRetentionConfigurationSection()
        {
            this.Variables = new List<DataRetentionVariableConfiguration>();
            this.RetentionRules = new List<DataRetentionRuleConfiguration>();
        }


        /// <summary>
        /// Gets the variables for this retention policy
        /// </summary>
        [XmlArray("vars"), XmlArrayItem("add"), JsonProperty("vars")]
        [DisplayName("Policy Variables"), Description("Specifies one or more dynamically calculated variables which are used in the include and exclude filters")]
        public List<DataRetentionVariableConfiguration> Variables { get; set; }

        /// <summary>
        /// Data retention rules
        /// </summary>
        [XmlArray("rules"), XmlArrayItem("add"), JsonProperty("rules")]
        [DisplayName("Policy Definitions"), Description("Specifies one or more retention policies which are applied whenever the retention service is run")]
        public List<DataRetentionRuleConfiguration> RetentionRules { get; set; }

        /// <summary>
        /// Represent as a string
        /// </summary>
        public override string ToString() => $"{this.RetentionRules?.Count ?? 0} Policies";

    }

    /// <summary>
    /// Retention variable
    /// </summary>
    [XmlType(nameof(DataRetentionVariableConfiguration), Namespace = "http://openiz.org/configuration")]
    public class DataRetentionVariableConfiguration
    {
        /// <summary>
        /// The name of the object
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name"), DisplayName("Variable Name"), Description("The name of the variable, accessed with $name in the filter expressions")]
        public string Name { get; set; }

        /// <summary>
        /// Expression to set
        /// </summary>
        [XmlText, JsonProperty("expr"), DisplayName("C# Expression"), Description("The C# expression which calculates the value of the variable")]
        public string Expression { get; set; }

        /// <summary>
        /// Get the specified delegate
        /// </summary>
        public Func<Object> CompileFunc(Dictionary<String, Func<Object>> variableFunc = null)
        {
            CompiledExpression<dynamic> exp = new CompiledExpression<dynamic>(this.Expression);
            exp.TypeRegistry = new TypeRegistry();
            exp.TypeRegistry.RegisterDefaultTypes();
            exp.TypeRegistry.RegisterType<Guid>();
            exp.TypeRegistry.RegisterType<TimeSpan>();
            exp.TypeRegistry.RegisterParameter("now", () => DateTime.Now); // because MONO is scumbag

            if (variableFunc != null)
                foreach (var fn in variableFunc)
                    exp.TypeRegistry.RegisterParameter(fn.Key, fn.Value);
            //exp.TypeRegistry.RegisterSymbol("data", expressionParm);
            //exp.ScopeCompile<TData>();
            //Func<TData, bool> d = exp.ScopeCompile<TData>();
            return exp.Compile();
        }
    }

    /// <summary>
    /// Identifies the action to take when the retained object is set
    /// </summary>
    public enum DataRetentionActionType
    {
        /// <summary>
        /// The object should be purged (deleted from the database)
        /// </summary>
        [XmlEnum("purge")]
        Purge = 0x1,
        /// <summary>
        /// The object should be obsoleted in the persistence layer
        /// </summary>
        [XmlEnum("obsolete")]
        Obsolete = 0x2,
        /// <summary>
        /// The object should be archived using the IDataArchiveService
        /// </summary>
        [XmlEnum("archive")]
        Archive = 0x4
    }

    /// <summary>
    /// Retention rule configuration
    /// </summary>
    [XmlType(nameof(DataRetentionRuleConfiguration), Namespace = "http://santedb.org/configuration")]
    public class DataRetentionRuleConfiguration
    {

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name"), DisplayName("Name"), Description("The unique name of the retention policy which will appear in logs")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        [XmlElement("type"), JsonProperty("type"), DisplayName("Applies To")]
        [Editor("SanteDB.Configuration.Editors.ResourceCollectionEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public ResourceTypeReferenceConfiguration ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the filter expressions the rule applies (i.e. objects matching this rule will be included)
        /// </summary>
        [XmlArray("includes"), XmlArrayItem("filter"), JsonProperty("includes"), DisplayName("Include Filter")]
        public String[] IncludeExpressions { get; set; }

        /// <summary>
        /// Gets or sets the objects which are excluded.
        /// </summary>
        [XmlArray("excludes"), XmlArrayItem("filter"), JsonProperty("excludes"), DisplayName("Exclude Filter")]
        public String[] ExcludeExpressions { get; set; }

        /// <summary>
        /// Dictates the action
        /// </summary>
        [XmlAttribute("action"), JsonProperty("action"), DisplayName("Retention Policy")]
        public DataRetentionActionType Action { get; set; }

        /// <summary>
        /// Gets as a string
        /// </summary>
        public override string ToString() => $"{this.Action} - {this.ResourceType}";
    }
}
