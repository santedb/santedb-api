﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Job manager service
    /// </summary>
    public interface IJobManagerService : IDaemonService
    {

        /// <summary>
        /// Add a job
        /// </summary>
        void AddJob(IJob jobType, TimeSpan elapseTime);

        /// <summary>
        /// Returns true if the job is registered
        /// </summary>
        bool IsJobRegistered(Type jobType);

        /// <summary>
        /// Gets the status of all jobs
        /// </summary>
        IEnumerable<IJob> Jobs { get; }

        /// <summary>
        /// Start a job
        /// </summary>
        /// <param name="job">The job to start</param>
        /// <param name="parameters">The parameters to pass to the job</param>
        /// <returns>True if the job started successfully</returns>
        void StartJob(IJob job, object[] parameters);

        /// <summary>
        /// Get this manager's instance of a job
        /// </summary>
        /// <param name="jobType">The job type to fetch</param>
        IJob GetJobInstance(String jobTypeName);
    }
}
