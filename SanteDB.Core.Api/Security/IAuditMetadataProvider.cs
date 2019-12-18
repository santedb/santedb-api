using SanteDB.Core.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Audit metadata provider
    /// </summary>
    public interface IAuditMetadataProvider
    {

        /// <summary>
        /// Gets the metadata for this audit 
        /// </summary>
        IDictionary<AuditMetadataKey, Object> GetMetadata();

    }
}
