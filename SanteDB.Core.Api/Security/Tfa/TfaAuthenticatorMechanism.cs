/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Security.Principal;

namespace SanteDB.Core.Security.Tfa
{
    /// <summary>
    /// Represents a two-factor mechanism which provides a code for an authenticator app
    /// </summary>
    public class TfaAuthenticatorMechanism : ITfaMechanism
    {

        /// <summary>
        /// Gets the mechanism ID for this implementation
        /// </summary>
        public static readonly Guid MechanismId = Guid.Parse("7566D010-FA4A-444C-A10A-0ADC648846CE");
        private readonly ITfaCodeProvider m_tfaCodeProvider;
        private readonly ITfaSecretManager m_tfaSecretManager;

        /// <summary>
        /// DI Constructor
        /// </summary>
        public TfaAuthenticatorMechanism(ITfaCodeProvider tfaCodeProvider, ITfaSecretManager secretManager)
        {
            this.m_tfaCodeProvider = tfaCodeProvider;
            this.m_tfaSecretManager = secretManager;
        }

        /// <inheritdoc/>
        public SanteDBHostType[] HostTypes => new SanteDBHostType[] {
            SanteDBHostType.Client,
            SanteDBHostType.Debugger,
            SanteDBHostType.Gateway,
            SanteDBHostType.Server
        };

        /// <inheritdoc/>
        public TfaMechanismClassification Classification => TfaMechanismClassification.Application;

        /// <inheritdoc/>
        public string SetupHelpText => UserMessageStrings.MFA_MECHANISM_AUTHENTICATOR_HELP;

        /// <inheritdoc/>
        public Guid Id => MechanismId;

        /// <inheritdoc/>
        public string Name => UserMessageStrings.MFA_MECHANISM_AUTHENTICATOR;

        /// <inheritdoc/>
        public string Send(IIdentity user)
        {
            return UserMessageStrings.MFA_MECHANISM_AUTHENTICATOR_SCAN_INSTRUCTION;
        }

        /// <inheritdoc/>
        public string BeginSetup(IIdentity user)
        {
            if (user is IClaimsIdentity ci)
            {
                // Remove the old 
                this.m_tfaSecretManager.RemoveTfaRegistration(ci, AuthenticationContext.Current.Principal);
                var code = this.m_tfaSecretManager.StartTfaRegistration(ci, 6, Rfc4226Mode.TotpThirtySecondInterval, AuthenticationContext.Current.Principal);
                var secret = this.m_tfaSecretManager.GetSharedSecret(ci);

                // HACK: Get the secret for sharing
                return $"otpauth://totp/{user.Name}?secret={secret.Base32Encode()}&issuer={ApplicationServiceContext.Current.ApplicationName ?? "SanteDB"} on {Environment.MachineName}";
            }
            else
            {
                throw new InvalidOperationException("Cannot setup TOTP authenticator to non-claims identity.");
            }
        }

        /// <inheritdoc/>
        public bool EndSetup(IIdentity user, String verificationCode)
        {
            if (user is IClaimsIdentity claimsIdentity)
            {
                return this.m_tfaSecretManager.FinishTfaRegistration(claimsIdentity, verificationCode, AuthenticationContext.Current.Principal);
            }
            else
            {
                throw new InvalidOperationException("Cannot setup TOTP authenticator to non-claims identity.");
            }
        }

        /// <inheritdoc/>
        public bool Validate(IIdentity user, string secret)
        {
            if (user is IClaimsIdentity ci)
            {
                return this.m_tfaCodeProvider.VerifyTfaCode(ci, secret);
            }
            else
            {
                throw new InvalidOperationException("Cannot validate TOTP authenticator to non-claims identity.");
            }
        }
    }
}
