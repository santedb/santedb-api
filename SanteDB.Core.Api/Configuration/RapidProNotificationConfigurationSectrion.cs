/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Configuration for the RapidPro notification service.
    /// </summary>
    [XmlType(nameof(RapidProNotificationConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class RapidProNotificationConfigurationSection : IConfigurationSection
    {
        /// <summary>
        /// RapidPro API key used to authenticate with the RapidPro API.
        /// </summary>
        [XmlAttribute("apiKey")]
        public String ApiKey { get; set; }

        /// <summary>
        /// The base address of the RapidPro API.
        /// </summary>
        [XmlAttribute("baseAddress")]
        public String BaseAddress { get; set; }

        /// <summary>
        /// The user agent to use when making requests to the RapidPro API.
        /// </summary>
        [XmlAttribute("userAgent")]
        public String UserAgent { get; set; }
    }
}
