using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
	/// Represents a contract for a wrapper service to allow interoperation between the 
    /// 
	/// </summary>
	public interface ISecurityInformationService
    {
        /// Get active policies for the specified securable type
        /// </summary>
        IEnumerable<SecurityPolicyInstance> GetActivePolicies(object securable);

        /// <summary>
        /// Change password of a principal
        /// </summary>
        void ChangePassword(String userName, String password, IPrincipal principal);

        /// <summary>
        /// Add users to roles
        /// </summary>
        void AddUsersToRoles(String[] users, String[] roles);

        /// <summary>
        /// Remove users from roles
        /// </summary>
        void RemoveUsersFromRoles(String[] users, String[] roles);
    }

}
