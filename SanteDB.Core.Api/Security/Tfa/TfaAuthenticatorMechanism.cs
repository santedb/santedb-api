using SanteDB.Core.i18n;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;

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
        public TfaMechanismClassification Classification=> TfaMechanismClassification.Application;

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
            if(user is IClaimsIdentity ci) 
            {
                // Remove the old 
                this.m_tfaSecretManager.RemoveTfaRegistration(ci, AuthenticationContext.Current.Principal);
                var code = this.m_tfaSecretManager.StartTfaRegistration(ci, 6, Rfc4226Mode.TotpThirtySecondInterval, AuthenticationContext.Current.Principal);
                var secret = this.m_tfaSecretManager.GetSharedSecret(ci);

                // HACK: Get the secret for sharing
                return $"otpauth://totp/{user.Name}?secret={secret.Base32Encode()}&issuer=SanteDB on {Environment.MachineName}";
            }
            else
            {
                throw new InvalidOperationException("Cannot setup TOTP authenticator to non-claims identity.");
            }
        }

        /// <inheritdoc/>
        public bool EndSetup(IIdentity user, String verificationCode)
        {
            if(user is IClaimsIdentity claimsIdentity)
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
            if(user is IClaimsIdentity ci)
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
