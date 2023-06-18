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
 * Date: 2023-5-19
 */
using System;

#pragma warning disable  CS1587
/// <summary>
/// The SanteDB.Core.Attributes namespace contains common attributes used by the SanteDB core API
/// to load and reflect information about plugins and their capabilities.
/// </summary>
#pragma warning restore  CS1587

namespace SanteDB.Core.Attributes
{
    /// <summary>
    /// Identifies the type/environment that the plugin contained within the assembly operates in
    /// </summary>
    [Flags]
    public enum PluginEnvironment
    {
        /// <summary>
        /// The plugin works on the server only
        /// </summary>
        Server = 0x1,

        /// <summary>
        /// The plugin works on the client environment only
        /// </summary>
        Client = 0x2,

        /// <summary>
        /// The plugin works either on server or client
        /// </summary>
        ServerOrMobile = Server | Client,
    }

    /// <summary>
    /// Attached to an <c>AssemblyInfo.cs</c> file to annotate a plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of plugin which the plugin is known to operate within
        /// </summary>
        public PluginEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the grouping of the plugin functionality
        /// </summary>
        public String Group { get; set; }

        /// <summary>
        /// Gets or sets the enabling of the plugin by default
        /// </summary>
        public bool EnableByDefault { get; set; }
    }
}