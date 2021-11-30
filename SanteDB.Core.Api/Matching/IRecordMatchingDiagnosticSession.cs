using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Matching
{
    /// <summary>
    /// Defines a diagnostics session
    /// </summary>
    public interface IRecordMatchingDiagnosticSession
    {
        /// <summary>
        /// Log the start of a diagnostic session
        /// </summary>
        /// <param name="configurationName">The configuration being used</param>
        void LogStart(String configurationName);

        /// <summary>
        /// Log the end of the match operation
        /// </summary>
        void LogEnd();

        /// <summary>
        /// Log the starting of a stage
        /// </summary>
        /// <param name="stageId">The stage identifier</param>
        void LogStartStage(String stageId);

        /// <summary>
        /// Log the end of a stage
        /// </summary>
        void LogEndStage();

        /// <summary>
        /// Log the start of an action
        /// </summary>
        /// <param name="counterTag">The tag of the counter to start logging</param>
        void LogStartAction(object counterTag);

        /// <summary>
        /// Log the end of the action
        /// </summary>
        void LogEndAction();

        /// <summary>
        /// Log a count
        /// </summary>
        /// <param name="objectTag">The object tag to set</param>
        /// <param name="count">The count of objects</param>
        void LogSample<T>(string objectTag, T count);

        /// <summary>
        /// Get the session data
        /// </summary>
        object GetSessionData();
    }
}