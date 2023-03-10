/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using SanteDB.Core.Security.Tfa;
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
        string StartTfaRegistration(IIdentity identity, int codeLength, Rfc4226Mode rfc4226Mode, IPrincipal principal);
        bool FinishTfaRegistration(IIdentity identity, string code, IPrincipal principal);
    }

    
}
