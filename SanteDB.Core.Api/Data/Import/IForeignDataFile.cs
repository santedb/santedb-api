using System;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a foreign data file
    /// </summary>
    public interface IForeignDataFile : IDisposable
    {

        /// <summary>
        /// Get the subset names
        /// </summary>
        /// <returns>The names of the subsets in the file</returns>
        IEnumerable<String> GetSubsetNames();

        /// <summary>
        /// Create a reader for a subset
        /// </summary>
        /// <param name="subsetName">The name of the subset - if the data file is a complex datafile like Excel this is the sheet name</param>
        /// <returns>The reader</returns>
        IForeignDataReader CreateReader(string subsetName = null);

        /// <summary>
        /// Create a writer which can add data to <paramref name="subsetName"/>
        /// </summary>
        /// <param name="subsetName">The name of the subset to create the writer for</param>
        /// <returns>The foreign dat writer</returns>
        IForeignDataWriter CreateWriter(string subsetName = null);


    }
}