using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Interface for a two factor code generator.
    /// </summary>
    public interface ITfaCodeGenerator
    {
        /// <summary>
        /// Generate a code for a specific user.
        /// </summary>
        /// <param name="identity">The identity to generate the code for.</param>
        /// <param name="address">Optional address to specifically gather a secret for. </param>
        /// <returns>The generated code for the identity.</returns>
        string GenerateTfaCode(IIdentity identity, string address = null);
    }

    /// <summary>
    /// Interface for a two factor code verifier. 
    /// </summary>
    public interface ITfaCodeVerifier
    {
        /// <summary>
        /// Verify a two factor code is correct for a given user
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="code"></param>
        /// <param name="timeProvided"></param>
        /// <returns></returns>
        bool VerifyTfaCode(IIdentity identity, string code, DateTimeOffset? timeProvided = null);
    }

    public interface ITfaCodeProvider : ITfaCodeGenerator, ITfaCodeVerifier
    {


    }

    public interface ITfaSecretManager
    {
        string StartTfaRegistration(IIdentity identity, string address, int codeLength, IPrincipal principal);
        bool FinishTfaRegistration(IIdentity identity, string address, string code, IPrincipal principal);
    }

    
}
