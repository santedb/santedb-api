using System;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a foreign data file
    /// </summary>
    public interface IForeignDataFile : IDisposable
    {

        /// <summary>
        /// Create a reader for a subset
        /// </summary>
        /// <param name="subsetName">The name of the subset - if the data file is a complex datafile like Excel this is the sheet name</param>
        /// <returns>The reader</returns>
        IForeignDataReader CreateReader(string subsetName = null);

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="subsetName"></param>
        /// <returns></returns>
        IForeignDataWriter CreateWriter(string subsetName = null);


    }
}