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
using SanteDB.Core.Exceptions;
using System;
using System.Xml.Serialization;

#pragma warning disable  CS1587
/// <summary>
/// The core business rules namespace in the SanteDB API is used to define common classes for expressing
/// issues detected by SanteDB's business rules engine.
/// </summary>
#pragma warning restore CS1587

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// The priority of the detected issue
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
    /// An issue raised by CDSS, Business Rules, or the persistence layer representing a constraint on the object
    /// </summary>
    /// <remarks>
    /// <para>In SanteDB, a detected issue is used to represent a business constraint violation (opposed to a software exception)
    /// which has impacted the ability of the SanteDB server software to process the requested action. Examples of detected issues can include:</para>
    /// <list type="bullet">
    ///     <item>Assigning an incorrect / unbound code to a field </item>
    ///     <item>Adding an inapporpriate relationship type (i.e. Mother linking an Organization to a Person)</item>
    ///     <item>Inappropriately assigning an identifier out of scope (i.e. a GLN on a Person)</item>
    ///     <item>Business rules trigger violations</item>
    /// </list>
    /// <para>In SanteDB any plugin can throw a <see cref="DetectedIssueException"/> wrapping a collection of issues, in the core iCDR and dCDR services, detected issues are commonly raised by:</para>
    /// <list type="bullet">
    ///     <item>The persistence layer (validating relationship types, identifier scopes, etc.)</item>
    ///     <item>The data quality business rules trigger (validating in-country minimum data sets, etc.)</item>
    ///     <item>The JavaScript business rules service (whenever a business rule validate() or trigger event throws an exception)</item>
    ///     <item>The Clinical Decision Support / CarePlanner (when raised by a protocol)</item>
    /// </list>
    /// </remarks>
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
        /// Creates a new detected issue with the specified field values
        /// </summary>
        /// <param name="id">The unique identifier (context specific) of the business rule violation</param>
        /// <param name="priority">The priority / seriousness of the detected issue being raised</param>
        /// <param name="text">The textual content of the detected issue</param>
        /// <param name="type">The codified type of the detected issue (examples: <see cref="DetectedIssueKeys"/>)</param>
        public DetectedIssue(DetectedIssuePriorityType priority, String id, String text, Guid type)
        {
            this.Id = id;
            this.Priority = priority;
            this.Text = text;
            this.TypeKey = type;
        }

        /// <summary>
        /// Gets or sets the identifier of the detected issue
        /// </summary>
        /// <remarks>The identifier of the detected issue should indicate to the catcher of the issue, the exact codified type of the issue. It should be unique
        /// within a context, but consistent in meaning (i.e. the identifier is more of a mnemonic for the issue).</remarks>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// Represents a detected issue priority
        /// </summary>
        [XmlAttribute("priority"), JsonProperty("priority")]
        public DetectedIssuePriorityType Priority { get; set; }

        /// <summary>
        /// Gets or sets the textual description of the detected issue.
        /// </summary>
        [XmlText, JsonProperty("text")]
        public String Text { get; set; }

        /// <summary>
        /// The type of detected issue (a concept key) which can be used to classify the detected issue
        /// </summary>
        /// <seealso cref="DetectedIssueKeys"/>
        [XmlAttribute("type"), JsonProperty("type")]
        public Guid TypeKey { get; set; }
    }
}