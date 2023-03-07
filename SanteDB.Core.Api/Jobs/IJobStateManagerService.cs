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

namespace SanteDB.Core.Jobs
{

    /// <summary>
    /// Represents a job state
    /// </summary>
    public interface IJobState
    {
        /// <summary>
        /// Gets or sets the 
        /// </summary>
        IJob Job { get; }

        /// <summary>
        /// Gets the text of the state of the job
        /// </summary>
        String StatusText { get; }

        /// <summary>
        /// Gets the progress of the job
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Gets the current state
        /// </summary>
        JobStateType CurrentState { get; }

        /// <summary>
        /// Gets the start time of the job
        /// </summary>
        DateTime? LastStartTime { get; }

        /// <summary>
        /// Gets the last stop time
        /// </summary>
        DateTime? LastStopTime { get; }

    }
    /// <summary>
    /// Contract for a service which manages job states
    /// </summary>
    public interface IJobStateManagerService
    {

        /// <summary>
        /// Set the status of the job
        /// </summary>
        /// <param name="job">The job for which the state should be set</param>
        /// <param name="state">The state of the job</param>
        void SetState(IJob job, JobStateType state);

        /// <summary>
        /// Set the progress of the job
        /// </summary>
        /// <param name="job">The job for which the state should be set</param>
        /// <param name="statusText">The text of the job</param>
        /// <param name="progress">The progress of the job</param>
        void SetProgress(IJob job, String statusText, float progress);

        /// <summary>
        /// Get the job status
        /// </summary>
        /// <param name="job">The job to get the status for</param>
        IJobState GetJobState(IJob job);

    }
}
