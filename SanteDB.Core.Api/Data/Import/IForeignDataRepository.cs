using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a class which manages foreign data import and export to/from SanteDB
    /// </summary>
    public interface IForeignDataRepository
    {

        /// <summary>
        /// Stages the foreign data stream in a place which can be accessed by SanteDB
        /// </summary>
        /// <param name="foreignDataStream">The stream containing the source foreign data</param>
        /// <param name="fileName">The tag of the foreign data import object</param>
        /// <returns>The description of the foreign data which was uploaded</returns>
        ForeignDataElementGroup Stage(Stream foreignDataStream, String fileName);

        /// <summary>
        /// The stream which represents the staged data
        /// </summary>
        /// <param name="fileName">The tag which was assigned to the staged data</param>
        /// <returns>The staged source data</returns>
        Stream Get(String fileName);

        /// <summary>
        /// Delete the staged data tag
        /// </summary>
        /// <param name="fileName">The tag which should be deleted</param>
        void DeleteStagedData(String fileName);

    }
}
