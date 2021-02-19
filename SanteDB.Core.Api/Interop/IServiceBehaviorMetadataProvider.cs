using SanteDB.Core.Interop.Description;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Interop
{


    /// <summary>
    /// Represents a service behavior metadata provider
    /// </summary>
    /// <remarks>
    /// If this interface is not present then metadata interchange interfaces (WSDL, Swagger, etc.) will use reflection.
    /// </remarks>
    public interface IServiceBehaviorMetadataProvider
    {

        /// <summary>
        /// Gets the resources supported by this endpoint
        /// </summary>
        ServiceDescription Description { get; }
    }
}
