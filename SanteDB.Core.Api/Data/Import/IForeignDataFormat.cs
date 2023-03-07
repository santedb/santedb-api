using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a foreign data mapper which can apply a <see cref="ForeignDataMap"/> against an input
    /// source of data.
    /// </summary>
    /// <remarks>These mappers are designed to parse incoming lines or data from a <see cref="System.IO.Stream"/> 
    /// and apply the mapping instructions on the <see cref="ForeignDataMap"/> supplied resulting in
    /// <see cref="IdentifiedData"/> instances representing the contents of the foreign data map</remarks>
    public interface IForeignDataFormat
    {

        /// <summary>
        /// Gets the file extension of this data format
        /// </summary>
        String FileExtension { get; }

        /// <summary>
        /// Open a <see cref="IForeignDataFile"/> from <paramref name="foreignDataStream"/>
        /// </summary>
        /// <returns>The created reader implementation</returns>
        /// <param name="foreignDataStream">The foreign data which should be used to open the reader</param>
        IForeignDataFile Open(Stream foreignDataStream);

    }
}
