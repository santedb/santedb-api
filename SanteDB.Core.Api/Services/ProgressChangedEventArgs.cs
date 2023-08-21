using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Event arguments for process changing event
    /// </summary>
    /// <remarks>This class allows the <see cref="IReportProgressChanged"/> class to notify listeners of the current
    /// status of an operation it is performing.</remarks>
    public class ProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the task idenitifier for the progress change to indicate a task or subtask that is reporting progress. Interfaces will group status updates by task identifier.
        /// </summary>
        public string TaskIdentifier { get; }

        /// <summary>
        /// Gets the progress of the event normalized between 0 and 1. A value of less than zero indicates indeterminate progress.
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Gets contextual information (such as text) which is appended to the progress on user interfaces or REST messages
        /// </summary>
        public string State { get; }

        /// <summary>
        /// Creates a new progress changed event args with no task identifier
        /// </summary>
        /// <param name="progress">The progress of the state change</param>
        /// <param name="state">The textual status of the state change</param>
        /// <remarks>This constructor is for backwards compatibility and will assign a default <see cref="TaskIdentifier"/>.
        /// Implementers should call <see cref="ProgressChangedEventArgs.ProgressChangedEventArgs(string, float, string)"/></remarks>
        [Obsolete("Use ProgressChangedEventArgs(string, float, string)")]
        public ProgressChangedEventArgs(float progress, string state)
        {
            this.Progress = progress;
            this.State = state;
            this.TaskIdentifier = String.Empty;
        }

        /// <summary>
        /// Creates a new progress changed event
        /// </summary>
        /// <param name="progress">The progress of the operation which is being reported on</param>
        /// <param name="state">The state object to report with the progress</param>
        /// <param name="taskIdentifier">The task identifier for the background state</param>
        public ProgressChangedEventArgs(string taskIdentifier, float progress, string state)
#pragma warning disable CS0618
            : this(progress, state)
#pragma warning restore CS0618
        {
            this.TaskIdentifier = taskIdentifier;
        }
    }
}
