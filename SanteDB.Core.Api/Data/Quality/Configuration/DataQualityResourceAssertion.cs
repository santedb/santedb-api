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
using SanteDB.Core.BusinessRules;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Represents a single assertion on a resource
    /// </summary>
    [XmlType(nameof(DataQualityResourceAssertion), Namespace = "http://santedb.org/configuration")]
    public class DataQualityResourceAssertion
    {

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name 
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority 
        /// </summary>
        [XmlAttribute("priority")]
        public DetectedIssuePriorityType Priority { get; set; }

        /// <summary>
        /// The evaluation
        /// </summary>
        [XmlAttribute("evaluation")]
        public AssertionEvaluationType Evaluation { get; set; }

        /// <summary>
        /// Gets or sets the expressions which are checked
        /// </summary>
        [XmlElement("expression")]
        public List<string> Expressions { get; set; }
    }

    /// <summary>
    /// Assertion evaluation type
    /// </summary>
    [XmlType(nameof(AssertionEvaluationType), Namespace = "http://santedb.org/configuration")]
    public enum AssertionEvaluationType
    {
        /// <summary>
        /// All of the expressions must evaluate to true
        /// </summary>
        [XmlEnum("all")]
        All, 
        /// <summary>
        /// Any of the expressions must evaluate to true
        /// </summary>
        [XmlEnum("any")]
        Any,
        /// <summary>
        /// None of the expressions should evaluate to true
        /// </summary>
        [XmlEnum("none")]
        None
    }
}