using SanteDB.Core.i18n;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Query
{

    /// <summary>
    /// Extension methods for the query filters
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Determines if <paramref name="securityEntity"/> has a claim <paramref name="claimType"/> 
        /// </summary>
        /// <param name="securityEntity">The security object for which the claim is being looked up</param>
        /// <param name="claimType">The type of the claim</param>
        /// <returns>The value of the claim value matches the value</returns>
        public static String ClaimLookup(this SecurityEntity securityEntity, String claimType)
        {

            var appServiceProvider = ApplicationServiceContext.Current;
            IEnumerable<IClaim> claimSource = null;
            switch(securityEntity)
            {
                case SecurityUser su:
                    claimSource = appServiceProvider.GetService<IIdentityProviderService>().GetClaims(su.UserName);
                    break;
                case SecurityApplication sa:
                    claimSource = appServiceProvider.GetService<IApplicationIdentityProviderService>().GetClaims(sa.Name);
                    break;
                case SecurityDevice sd:
                    claimSource = appServiceProvider.GetService<IDeviceIdentityProviderService>().GetClaims(sd.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(SecurityUser), securityEntity.GetType()));
            }

            // Lookup claim value
            return claimSource.Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)).Select(o=>o.Value).FirstOrDefault();

        }
    }
}
