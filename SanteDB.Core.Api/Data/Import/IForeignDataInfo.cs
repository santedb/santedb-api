using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Foreign data information wrapper
    /// </summary>
    public interface IForeignDataInfo : IIdentifiedResource
    {

        /// <summary>
        /// Gets the name of the foreign data information
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the status of the foreign data information
        /// </summary>
        ForeignDataStatus Status { get; }

        /// <summary>
        /// Gets the foreign data map that was used to import the data
        /// </summary>
        ForeignDataMap ForeignDataMap { get; }

        /// <summary>
        /// Get the issues with the imported data
        /// </summary>
        /// <returns>The detected issues for the imported data</returns>
        IEnumerable<DetectedIssue> GetIssues();

        /// <summary>
        /// Add an issue to this foreign data information class
        /// </summary>
        /// <param name="issue">The issue to add to this foreign da</param>
        void AddIssue(DetectedIssue issue);

        /// <summary>
        /// Get the source stream of the data
        /// </summary>
        /// <returns>The source stream</returns>
        Stream GetSource();

        /// <summary>
        /// Get the rejected file data
        /// </summary>
        Stream GetRejects();

    }

    /// <summary>
    /// Represents the foreign data status
    /// </summary>
    public enum ForeignDataStatus
    {
        /// <summary>
        /// The foreign data information has been received and staged
        /// </summary>
        Staged,
        /// <summary>
        /// The foreign data is scheduled for execution
        /// </summary>
        Scheduled,
        /// <summary>
        /// The foreign data is being imported
        /// </summary>
        Running,
        /// <summary>
        /// The foreign data import was completed with no errors
        /// </summary>
        CompletedSuccessfully,
        /// <summary>
        /// The foreign data import was completed with errors
        /// </summary>
        CompletedWithErrors
    }

}