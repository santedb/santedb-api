using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Interop.Description
{
    /// <summary>
    /// Represents the service descriptor
    /// </summary>
    public class ServiceDescription
    {

        /// <summary>
        /// Operations
        /// </summary>
        public ServiceDescription()
        {
            this.Operations = new List<ServiceOperationDescription>();
        }

        /// <summary>
        /// Gets the operations supported by this service
        /// </summary>
        public IList<ServiceOperationDescription> Operations { get; }

    }
}
