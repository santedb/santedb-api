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
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.BusinessRules
{
	/// <summary>
	/// Detected issue priority
	/// </summary>
	public enum DetectedIssuePriorityType : int
    {
        /// <summary>
        /// The issue is an error, processing cannot continue until the issue is corrected
        /// </summary>
		Error = 1,
        /// <summary>
        /// The issue is a warning (dismissable)
        /// </summary>
		Warning = 2,
        /// <summary>
        /// The issue is for information, processing will continue
        /// </summary>
		Information = 4
    }

    /// <summary>
    /// Represents a detected issue
    /// </summary>
    [JsonObject(nameof(DetectedIssue))]
    [XmlType(nameof(DetectedIssue), Namespace = "http://santedb.org/issue")]
    public class DetectedIssue
    {


        /// <summary>
        /// Default ctor for detected issues
        /// </summary>
        public DetectedIssue()
        {

        }

        /// <summary>
        /// Creates a new detected issue
        /// </summary>
        public DetectedIssue(DetectedIssuePriorityType priority, String id, String text, Guid type)
        {
            this.Id = id;
            this.Priority = priority;
            this.Text = text;
            this.TypeKey = type;
        }



        /// <summary>
        /// Gets or sets the id
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// Represents a detected issue priority
        /// </summary>
        [XmlAttribute("priority"), JsonProperty("priority")]
        public DetectedIssuePriorityType Priority { get; set; }

        /// <summary>
        /// Text related to the issue
        /// </summary>
        [XmlText, JsonProperty("text")]
        public String Text { get; set; }

        /// <summary>
        /// The type of issue (a concept)
        /// </summary>
        /// <seealso cref="DetectedIssueKeys"/>
        [XmlAttribute("type"), JsonProperty("type")]
        public Guid TypeKey { get; set; }
        
    }
}
