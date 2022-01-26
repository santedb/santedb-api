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
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Defines a regular schedule system background task
    /// </summary>
    /// <example>
    /// <code language="cs">
    /// <![CDATA[
    ///     public class HelloWorldJob : IJob {
    ///         
    ///         private bool m_cancelRequested = false;
    ///         
    ///         public Guid Id => Guid.NewGuid();
    ///         
    ///         public string Name => "Hello World Job!";
    ///         
    ///         public bool CanCancel => true;
    ///         
    ///         public IDictionary<String, Type> Parameters => null;
    ///         
    ///         public JobStateType CurrentState { get; private set; }
    ///         
    ///         public DateTime? LastStarted { get; private set; }
    ///         
    ///         public DateTime? LastFinished { get; private set; }
    ///         
    ///         public void Cancel() {
    ///             this.m_cancelRequested = true;
    ///         }
    ///         
    ///         public void Run(object sender, EventArgs e, object[] parameters) {
    ///             try {
    ///                 this.CurrentState = JobStateType.Running;
    ///                 this.LastStart = DateTime.Now;
    ///                 this.m_cancelRequested = false;
    ///                 while(!this.m_cancelRequested) {
    ///                     Console.Writeline("Hello World!");
    ///                 }
    ///                 if(this.m_cancelRequested) {
    ///                     this.CurrentState = JobStateType.Cancelled;
    ///                 }
    ///                 else {
    ///                     this.CurrentState = JobStateTime.Complete;
    ///                 }
    ///                 this.LastFinished = DateTime.Now;
    ///             }
    ///             catch(Exception e) {
    ///                 this.CurrentState = JobStateType.Aborted;
    ///             }
    ///         }
    ///     }
    ///     
    /// ]]>
    /// </code>
    /// </example>
    public interface IJob
    {

        /// <summary>
        /// A unique identifier for this job
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The name of the job
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the description of the job
        /// </summary>
        String Description { get; }

        /// <summary>
        /// True if the job can be cancelled
        /// </summary>
        bool CanCancel { get; }

        /// <summary>
        /// Execute the job
        /// </summary>
        void Run(object sender, EventArgs e, object[] parameters);

        /// <summary>
        /// Cancel the job
        /// </summary>
        void Cancel();

        /// <summary>
        /// Gets the current status of the job
        /// </summary>
        JobStateType CurrentState { get; }

        /// <summary>
        /// Get the parameter definitions
        /// </summary>
        IDictionary<String, Type> Parameters { get; }

        /// <summary>
        /// Gets the time the job last started
        /// </summary>
        DateTime? LastStarted { get; }

        /// <summary>
        /// Gets the time the job last finished
        /// </summary>
        DateTime? LastFinished { get; }

    }


    /// <summary>
    /// Job which reports progress
    /// </summary>
    public interface IReportProgressJob : IJob
    {
        /// <summary>
        /// Gets the progress of the job
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Get the status text of the job
        /// </summary>
        string StatusText { get; }
    }
}
