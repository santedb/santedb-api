using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// An interface capable of generating unique codes for two factor verification and they validating the codes generated.
    /// 
    /// It is intended that the code is transported to the user after generation and then the user will relay the code back using a different communication channel (hence, two-factor).
    /// </summary>
    public interface ITfaCodeProvider
    {
        /// <summary>
        /// Generate a code for a specific user.
        /// </summary>
        /// <param name="identity">The identity to generate the code for.</param>
        /// <param name="address">Optional address to specifically gather a secret for. </param>
        /// <returns>The generated code for the identity.</returns>
        string GenerateTfaCode(IIdentity identity);
        /// <summary>
        /// Verify a two factor code is correct for a given user
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="code"></param>
        /// <param name="timeProvided"></param>
        /// <returns></returns>
        bool VerifyTfaCode(IIdentity identity, string code, DateTimeOffset? timeProvided = null);
    }

    public interface ITfaSecretManager
    {
        string StartTfaRegistration(IIdentity identity, int codeLength, IPrincipal principal);
        bool FinishTfaRegistration(IIdentity identity, string code, IPrincipal principal);
    }

    
}
