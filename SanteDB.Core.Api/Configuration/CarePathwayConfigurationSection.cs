/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-12-12
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Configuration for the care pathway configuration section
    /// </summary>
    [XmlType(nameof(CarePathwayConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class CarePathwayConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// When true enables automatic enrolment
        /// </summary>
        [XmlAttribute("autoEnrollment"), JsonProperty("autoEnrollment")]
        [DisplayName("Enabled Auto Enrolment")]
        [Description("When enabled, care pathways with automatic enrolment will be automatically enroll patients matching the eligibility criteria")]
        public bool EnableAutoEnrollment { get; set; }
    }
}
