using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// An interface that resolves references to a file
    /// </summary>
    public interface IReferenceResolver
    {

        /// <summary>
        /// Resolve the specified reference
        /// </summary>
        /// <param name="reference">The reference path</param>
        /// <returns>The resolved reference</returns>
        Stream ResolveAsStream(String reference);

        /// <summary>
        /// Resolve the data at <paramref name="reference"/> as a string
        /// </summary>
        /// <param name="reference">The reference to resolve</param>
        /// <returns>The resolved reference</returns>
        String ResolveAsString(String reference);

    }
}
