using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Foreign data transformer uses a <see cref="ForeignDataMap"/> to map data to/from an external format
    /// </summary>
    public interface IForeignDataTransformer
    {

        /// <summary>
        /// Try to transform the <paramref name="foreignDataReader"/> into a <paramref name="transformedRecord"/>
        /// </summary>
        /// <param name="foreignDataReader">The foreign data record read from a <see cref="IForeignDataReader"/></param>
        /// <param name="transformedRecord">The transformed record</param>
        /// <param name="transformIssue">The transformation issues</param>
        /// <returns>True if the record was transformed successfully</returns>
        bool TryTransform(IForeignDataReader foreignDataReader, out IdentifiedData transformedRecord, out DetectedIssue transformIssue);

        /// <summary>
        /// Try to transform <paramref name="identifiedData"/> into a <paramref name="foreignDataWriter"/>
        /// </summary>
        /// <param name="identifiedData">The identified data to be transformed</param>
        /// <param name="foreignDataWriter">The foreign data record resulting from the transform</param>
        /// <param name="transformIssue">The issues with transforming the record</param>
        /// <returns>True if the transform was successful</returns>
        bool TryTransform(IdentifiedData identifiedData, out IForeignDataWriter foreignDataWriter, out DetectedIssue transformIssue);


    }
}
