using System;
using System.Collections.Generic;
using System.Text;

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
