using SanteDB.Core.BusinessRules;
using System;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a writer for the foreign data format
    /// </summary>
    public interface IForeignDataWriter : IDisposable
    {
        /// <summary>
        /// Write the <paramref name="foreignDataRecord"/> to the writer
        /// </summary>
        /// <param name="foreignDataRecord">The foreign data record to be written</param>
        /// <returns>True if the record was written</returns>
        bool WriteRecord(IForeignDataRecord foreignDataRecord);
    }
}