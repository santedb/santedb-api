using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Defines a reader which can read a format of data
    /// which is alien to SanteDB and convert it into
    /// the equivalent SanteDB records.
    /// </summary>
    public interface IForeignDataConverter
    {

        /// <summary>
        /// Gets the file extension of objects which this foreign data converter
        /// is expected to convert
        /// </summary>
        String Extension { get; }

        /// <summary>
        /// Gets a structure definition of the shape of the alien data
        /// in the specified <paramref name="inStream"/>
        /// </summary>
        /// <param name="inStream">The input stream containing a sample of alien data</param>
        /// <returns>A description of the foreign data source</returns>
        ForeignDataElementGroup GetDescriptor(Stream inStream);

        /// <summary>
        /// Converts the contents of the foreign data format into SanteDB
        /// objects
        /// </summary>
        /// <param name="inStream">The source stream containing the alien data</param>
        /// <param name="dataMap">The data map to be used to convert the foreign data into SanteDB objects</param>
        /// <returns>The converted SanteDB objects</returns>
        IEnumerable<IdentifiedData> Convert(Stream inStream, ForeignDataMap dataMap);

        /// <summary>
        /// Converts a collection of SanteDB objects to the foreign data format
        /// </summary>
        /// <param name="inData">The SanteDB objects to be converted</param>
        /// <param name="dataMap">The data mapping to convert to</param>
        /// <returns>The converted object</returns>
        Stream Convert(IEnumerable<IdentifiedData> inData, ForeignDataMap dataMap);
    }
}
