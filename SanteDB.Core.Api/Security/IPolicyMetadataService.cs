using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
	/// Represents a contract for a policy information service
	/// </summary>
	public interface IPolicyMetadataService
    {
        /// Get active policies for the specified securable type
        /// </summary>
        IEnumerable<SecurityPolicyInstance> GetActivePolicies(object securable);

    }

}
