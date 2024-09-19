/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic feature configuration structure which can be used when a feature wants to re-organize the manner in which it
    /// displays the inputs for the configuration
    /// </summary>
    /// <remarks>This class is used as the Configuration property value for features which either have combination configuration (i.e. the feature
    /// needs to configure many sections and cannot simply expose them all into a single property grid), or when the feature controls
    /// multiple services.</remarks>
    /// <example>
    /// <code language="cs" title="Expose two Configuration Sections">
    /// <![CDATA[
    ///    public FeatureInstallState QueryState(SanteDBConfiguration configuration) {
    ///         var genericFeature = new GenericFeatureConfiguration();
    ///         genericFeature.Categories.Add("settings", new string[] { "SettingA", "SettingB" });
    ///         genericFeature.Values.Add("SettingA", configuration.GetSection<SectionA>());
    ///         genericFeature.Values.Add("SettingB", confguration.GetSection<SectionB>());
    ///         genericFeature.Options.Add("SettingA", () => ConfigurationOptionType.Object);
    ///         genericFeature.Options.Add("SettingB", () => ConfigurationOptionType.Object);
    ///         genericFeature.Options.Add("Use Which?", () => new string[] { "Setting A", "Setting B" });
    ///         genericFeature.Values.Add("Use Which?", configuration.GetSection<SettingA>()?.Enabled = "Setting A" : "Setting B");
    ///
    ///    }
    /// ]]>
    /// </code>
    /// </example>
    public class GenericFeatureConfiguration
    {
        /// <summary>
        /// Generic feature configuration
        /// </summary>
        public GenericFeatureConfiguration()
        {
            this.Options = new Dictionary<string, Func<object>>();
            this.Values = new Dictionary<string, object>();
            this.Categories = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Represents the setting groupings or categories in the configuration object.
        /// </summary>
        /// <remarks>The key is the group heading, and the value is a collection of named options which appear in the group</remarks>
        /// <seealso cref="Options"/>
        public Dictionary<string, string[]> Categories { get; }

        /// <summary>
        /// Gets the configuration options for this generic setting
        /// </summary>
        /// <remarks>The key is the name of the option setting, the value is function which returns either a <see cref="ConfigurationOptionType"/> indicating the type of
        /// data, or an enumerable of the allowed options.</remarks>
        /// <seealso cref="ConfigurationOptionType"/>
        public Dictionary<string, Func<object>> Options { get; }

        /// <summary>
        /// Gets the current set configuration values as they are in the configuration file
        /// </summary>
        public Dictionary<string, object> Values { get; }
    }
}