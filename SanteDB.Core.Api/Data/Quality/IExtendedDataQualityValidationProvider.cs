using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Quality
{

    /// <summary>
    /// Non-generic version of the exnteded data quality validation provider
    /// </summary>
    public interface IExtendedDataQualityValidationProvider
    {

        /// <summary>
        /// Gets the type of data that this validation provider can validae
        /// </summary>
        Type[] SupportedTypes { get; }

        /// <summary>
        /// Perform validation on the specified <paramref name="objectToValidate"/>
        /// </summary>
        /// <param name="objectToValidate">The object instance to be validated</param>
        /// <returns>The detected issues from this validation provider from <paramref name="objectToValidate"/></returns>
        IEnumerable<DetectedIssue> Validate(object objectToValidate);
    }

}
