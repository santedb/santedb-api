/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Security.Tfa;
using System;
using System.Security;
using System.Security.Principal;

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

    /// <summary>
    /// Represents a secret manager which can manage the TFA secret registration with a user
    /// </summary>
    public interface ITfaSecretManager
    {
        /// <summary>
        /// Register an RFC4226 MFA secret setting to <paramref name="identity"/>
        /// </summary>
        /// <param name="identity">The identity to which the secret setting should be added</param>
        /// <param name="codeLength">The code length</param>
        /// <param name="rfc4226Mode">The secret mode (which dictates the generation and validation of the RFC secret)</param>
        /// <param name="principal">The principal which is adding the secret configuration</param>
        /// <returns>The registration data</returns>
        string StartTfaRegistration(IIdentity identity, int codeLength, Rfc4226Mode rfc4226Mode, IPrincipal principal);

        /// <summary>
        /// Remove a TFA registration from <paramref name="identity"/>
        /// </summary>
        /// <param name="identity">The identity from which the TFA setting should be removed</param>
        /// <param name="principal">The principal which is destroying the TFA setting</param>
        void RemoveTfaRegistration(IIdentity identity, IPrincipal principal);

        /// <summary>
        /// Completes an RFC4226 MFA secret registration
        /// </summary>
        /// <param name="identity">The identity to which the secret registraiton is to be completed</param>
        /// <param name="code">The code which was used to confirm the MFA secret setting</param>
        /// <param name="principal">The prinicpal which is completing the registration process</param>
        /// <returns>True if the registration was successfully completed</returns>
        bool FinishTfaRegistration(IIdentity identity, string code, IPrincipal principal);

        /// <summary>
        /// Get the secret for claim 
        /// </summary>
        /// <param name="identity">The identity which is getting the secret</param>
        /// <param name="principal">The principal getting the secret</param>
        /// <returns>The secret claim which backs the TFA mechanism</returns>
        /// <remarks>This method should only be called between <see cref="StartTfaRegistration(IIdentity, int, Rfc4226Mode, IPrincipal)"/> and 
        /// <see cref="FinishTfaRegistration(IIdentity, string, IPrincipal)"/> otherwise as <see cref="SecurityException"/> may be thrown</remarks>
        byte[] GetSharedSecret(IIdentity identity);
    }


}
