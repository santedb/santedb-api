/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Represents an exception which contains a series of detected issue events
    /// </summary>
    public class DetectedIssueException : Exception
    {

        /// <summary>
        /// Gets the list of issues set by the BRE 
        /// </summary>
        public IEnumerable<DetectedIssue> Issues { get; private set; }

        /// <summary>
        /// Get the message for the error
        /// </summary>
        public override string Message => $"{base.Message} - [{String.Join(",", this.Issues.Select(i=>$"{i.Priority}: {i.Text}"))}]";

        /// <summary>
        /// Creates a new detected issue exception
        /// </summary>
        public DetectedIssueException()
        {

        }

        /// <summary>
        /// Creates a new detected issue exception with the specified <paramref name="issues"/> 
        /// </summary>
        public DetectedIssueException(IEnumerable<DetectedIssue> issues, Exception cause) : this(issues, "Business Rules Violation", cause)
        {

        }

        /// <summary>
        /// Creates a new detected issue exception with the specified <paramref name="issues"/> <paramref name="message"/> and causal exception (<paramref name="innerException"/>)
        /// </summary>
        public DetectedIssueException(IEnumerable<DetectedIssue> issues, String message, Exception innerException) : base(message, innerException)
        {
            this.Issues = issues;
        }

        /// <summary>
        /// Creates a new detected issue exception with the specified issue list
        /// </summary>
        public DetectedIssueException(IEnumerable<DetectedIssue> issues) : this(issues, "Business Rules Violation", null)
        {
        }

        /// <summary>
        /// Detected issue exception
        /// </summary>
        public DetectedIssueException(DetectedIssue issue) : this(new List<DetectedIssue>() { issue })
        {

        }

        /// <summary>
        /// Detected issue exception
        /// </summary>
        public DetectedIssueException(DetectedIssue issue, Exception cause) : this(new List<DetectedIssue>() { issue }, cause)
        {

        }

        /// <summary>
        /// Creates a new detected issue exception
        /// </summary>
        /// <param name="priority">The priority of the detected issue</param>
        /// <param name="id">The unique identifier of the issue</param>
        /// <param name="text">The textual information on the issue</param>
        /// <param name="type">The type of issue</param>
        /// <param name="cause">What caused this issue</param>
        public DetectedIssueException(DetectedIssuePriorityType priority, String id, String text, Guid type, Exception cause) : base(text, cause)
        {
            this.Issues = new List<DetectedIssue>()
            {
                new DetectedIssue(priority, id, text, type)
            };
        }

        /// <summary>
        /// Write to string
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder("BRE Violations/Detected Issues:");
            foreach (var i in this.Issues)
#if DEBUG
                sb.AppendFormat("\r\n{0}- {1}", i.Priority, i.Text);
#else
                sb.AppendFormat("\r\n{0}- {1}", i.Priority, i.Text);
#endif

            sb.AppendFormat("\r\n\r\nAt: {0}", this.StackTrace);
            return sb.ToString();
        }

    }
}
