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
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Foreign data information wrapper
    /// </summary>
    public interface IForeignDataSubmission : IIdentifiedResource, INonVersionedData
    {

        /// <summary>
        /// Gets the name of the foreign data information
        /// </summary>
        [QueryParameter("name")]
        String Name { get; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [QueryParameter("description")]
        String Description { get; }

        /// <summary>
        /// Gets the status of the foreign data information
        /// </summary>
        [QueryParameter("status")]
        ForeignDataStatus Status { get; }

        /// <summary>
        /// Gets the foreign data map that was used to import the data
        /// </summary>
        [QueryParameter("map")]
        Guid ForeignDataMapKey { get; }

        /// <summary>
        /// Gets the parameter values
        /// </summary>
        IDictionary<String, String> ParameterValues { get; }

        /// <summary>
        /// Get the issues with the imported data
        /// </summary>
        /// <returns>The detected issues for the imported data</returns>
        IEnumerable<DetectedIssue> Issues { get; }

        /// <summary>
        /// Get the source stream of the data
        /// </summary>
        /// <returns>The source stream</returns>
        Stream GetSourceStream();

        /// <summary>
        /// Get the rejected file data
        /// </summary>
        Stream GetRejectStream();

    }

    /// <summary>
    /// Represents the foreign data status
    /// </summary>
    public enum ForeignDataStatus
    {
        /// <summary>
        /// The foreign data information has been received and staged
        /// </summary>
        Staged = 0,
        /// <summary>
        /// The foreign data is scheduled for execution
        /// </summary>
        Scheduled = 1,
        /// <summary>
        /// The foreign data is being imported
        /// </summary>
        Running = 2,
        /// <summary>
        /// The foreign data import was completed with no errors
        /// </summary>
        CompletedSuccessfully = 3,
        /// <summary>
        /// The foreign data import was completed with errors
        /// </summary>
        CompletedWithErrors = 4,
        /// <summary>
        /// The import was rejected
        /// </summary>
        Rejected = 5
    }

}