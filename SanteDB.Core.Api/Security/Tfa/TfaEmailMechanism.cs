/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model.Security;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;

namespace SanteDB.Core.Security.Tfa
{
    /// <summary>
    /// A <see cref="ITfaMechanism"/> which uses EMAIL as the communication 
    /// </summary>
    public class TfaEmailMechanism : ITfaMechanism
    {
        /// <summary>
        /// Gets the mechanism ID for this implementation
        /// </summary>
        public static readonly Guid MechanismId = Guid.Parse("D919457D-E015-435C-BD35-42E425E2C60C");
        private static readonly string s_TemplateName = "org.santedb.notifications.mfa.email";
        readonly INotificationService _NotificationService;
        readonly ITfaCodeProvider _TfaCodeProvider;
        private readonly ITfaSecretManager _TfaSecretManager;
        readonly ISecurityRepositoryService _SecurityRepository;
        readonly IRepositoryService<SecurityUser> _SecurityUserRepository;

        /// <summary>
        /// DI Constructor
        /// </summary>
        public TfaEmailMechanism(INotificationService notificationService, ITfaCodeProvider tfaCodeProvider, ITfaSecretManager secretManager, ISecurityRepositoryService securityRepository, IRepositoryService<SecurityUser> securityUserRepository)
        {
            _NotificationService = notificationService;
            _TfaCodeProvider = tfaCodeProvider;
            _TfaSecretManager = secretManager;
            _SecurityRepository = securityRepository;
            _SecurityUserRepository = securityUserRepository;

        }

        /// <inheritdoc/>
        public Guid Id => MechanismId;

        /// <inheritdoc/>
        public string Name => "org.santedb.tfa.email";

        /// <inheritdoc/>
        public SanteDBHostType[] HostTypes => new SanteDBHostType[] {
            SanteDBHostType.Server
        };

        /// <inheritdoc/>
        public TfaMechanismClassification Classification => TfaMechanismClassification.Message;

        /// <inheritdoc/>
        public string SetupHelpText => "org.santedb.tfa.email.setup";

        /// <inheritdoc/>
        public string BeginSetup(IIdentity user)
        {
            if (user is IClaimsIdentity ci)
            {
                this._TfaSecretManager.RemoveTfaRegistration(ci, AuthenticationContext.Current.Principal);
                var email = this.GetEmailAddressOrThrow(ci);
                var secret = _TfaSecretManager.StartTfaRegistration(ci, 6, Rfc4226Mode.TotpTenMinuteInterval, AuthenticationContext.SystemPrincipal);
                return this.SendCodeNotification(email, secret, ci.Name);
            }
            else
            {
                throw new InvalidOperationException("Cannot send notification to non-claims identity.");
            }
        }

        /// <inheritdoc/>
        public bool EndSetup(IIdentity user, string verificationCode)
        {
            if (user is IClaimsIdentity ci)
            {
                var email = this.GetEmailAddressOrThrow(ci);
                var result = _TfaSecretManager.FinishTfaRegistration(ci, verificationCode, AuthenticationContext.SystemPrincipal);

                if (result)
                {
                    var userentity = _SecurityRepository.GetUser(user);
                    userentity.EmailConfirmed = true;
                    _SecurityUserRepository.Save(userentity);
                }

                return result;
            }
            else
            {
                throw new InvalidOperationException("Cannot send notification to non-claims identity.");
            }
        }

        /// <inheritdoc/>
        public string Send(IIdentity user)
        {
            if (null == user)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is IClaimsIdentity ci)
            {
                var email = this.GetEmailAddressOrThrow(ci);
                string secret = _TfaCodeProvider.GenerateTfaCode(ci);
                return this.SendCodeNotification(email, secret, user.Name);
            }
            else
            {
                throw new InvalidOperationException("Cannot send notification to non-claims identity.");
            }
        }

        /// <summary>
        /// Get the e-mail address for <paramref name="claimsIdentity"/>
        /// </summary>
        private string GetEmailAddressOrThrow(IClaimsIdentity claimsIdentity)
        {
            var email = claimsIdentity.GetFirstClaimValue(SanteDBClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                var identity = AuthenticationContext.Current.GetUserIdentity();

                var securityuser = _SecurityRepository.GetUser(identity);

                email = securityuser?.Email;
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("E-Mail TFA requires e-mail address registered");
            }
            else if (!email.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                return "mailto:" + email;
            }
            return email;
        }

        /// <summary>
        /// Send the actual notification e-mail to the user
        /// </summary>
        private string SendCodeNotification(String email, string code, string userName)
        {
            try
            {
                var templatemodel = new Dictionary<String, Object>()
                {
                    {"user",  userName },
                    { "code", code }
                };

                _NotificationService.SendTemplatedNotification(new[] { email }, s_TemplateName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, templatemodel, null, false);
                var censoredemail = email.Split('@')[1];
                return $"Code sent to ******@{censoredemail}";
            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            {
                throw new Exception("Error sending notification for tfa email.", ex);
            }
        }

        /// <inheritdoc/>
        public bool Validate(IIdentity user, string secret)
        {
            if (null == user)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new ArgumentException("Invalid secret", nameof(secret));
            }

            return _TfaCodeProvider.VerifyTfaCode(user, secret, DateTimeOffset.UtcNow);
        }
    }
}
