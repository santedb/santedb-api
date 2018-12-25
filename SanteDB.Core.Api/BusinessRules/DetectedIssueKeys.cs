﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-12-1
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// Detected issue type keys
    /// </summary>
    public static class DetectedIssueKeys
    {

        /// <summary>
        /// Password failed validation
        /// </summary>
        public static readonly Guid SecurityIssue = Guid.Parse("1a4ff454-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Password failed validation
        /// </summary>
        public static readonly Guid FormalConstraintIssue = Guid.Parse("1a4ff6f2-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Codification issue
        /// </summary>
        public static readonly Guid CodificationIssue = Guid.Parse("1a4ff850-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Some other issue
        /// </summary>
        public static readonly Guid OtherIssue = Guid.Parse("1a4ff986-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Already performed
        /// </summary>
        public static readonly Guid AlreadyDoneIssue = Guid.Parse("1a4ffab2-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Invalid data 
        /// </summary>
        public static readonly Guid InvalidDataIssue = Guid.Parse("1a4ffcec-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Business rule violation
        /// </summary>
        public static readonly Guid BusinessRuleViolationIssue = Guid.Parse("1a4ffe40-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Business rule violation
        /// </summary>
        public static readonly Guid SafetyConcernIssue = Guid.Parse("1a4fff6c-f54f-11e8-8eb2-f2801f1b9fd1");

    }
}
