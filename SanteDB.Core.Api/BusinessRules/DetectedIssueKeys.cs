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
using System;

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// Common detected issue type keys
    /// </summary>
    /// <remarks>This class holds readonly UUIDs (constants) for the built-in detected issue
    /// types in SanteDB.</remarks>
    public static class DetectedIssueKeys
    {
        /// <summary>
        /// The detected issue represents a security issue such as password reset violation, inappropriate access, etc.
        /// </summary>
        public static readonly Guid SecurityIssue = Guid.Parse("1a4ff454-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The detected issue represents a formal constraint issues such as assignment of an inappropriate object type to a relationship
        /// </summary>
        public static readonly Guid FormalConstraintIssue = Guid.Parse("1a4ff6f2-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The detected issue represents the inappropriate use of a code in a field
        /// </summary>
        public static readonly Guid CodificationIssue = Guid.Parse("1a4ff850-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The detected issue represents some other, unclassified violation
        /// </summary>
        public static readonly Guid OtherIssue = Guid.Parse("1a4ff986-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The action being executed was already performed, or does not need to be redone
        /// </summary>
        public static readonly Guid AlreadyDoneIssue = Guid.Parse("1a4ffab2-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The action which was requested to be done had an invalid data element provided (example: last menstrual period for a male)
        /// </summary>
        public static readonly Guid InvalidDataIssue = Guid.Parse("1a4ffcec-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The detected issue was raised because of a custom business rule violaton.
        /// </summary>
        public static readonly Guid BusinessRuleViolationIssue = Guid.Parse("1a4ffe40-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The issue is being raised because the original action would raise a concern for patient safety
        /// </summary>
        public static readonly Guid SafetyConcernIssue = Guid.Parse("1a4fff6c-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// The detected issue is a patient privacy issue
        /// </summary>
        public static readonly Guid PrivacyIssue = Guid.Parse("a799d33d-2326-4beb-aa27-6ff82bd0e217");
    }
}