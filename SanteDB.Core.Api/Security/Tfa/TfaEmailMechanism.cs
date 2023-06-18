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
 * Date: 2023-5-19
 */
using SanteDB.Core.Notifications;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
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

        /// <summary>
        /// DI Constructor
        /// </summary>
        public TfaEmailMechanism(INotificationService notificationService, ITfaCodeProvider tfaCodeProvider, ITfaSecretManager secretManager)
        {
            _NotificationService = notificationService;
            _TfaCodeProvider = tfaCodeProvider;
            _TfaSecretManager = secretManager;
        }

        /// <inheritdoc/>
        public Guid Id => MechanismId;

        /// <inheritdoc/>
        public string Name => "org.santedb.tfa.email";

        /// <inheritdoc/>
        public string Send(IIdentity user)
        {
            if (null == user)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is IClaimsIdentity ci)
            {
                var email = ci.GetFirstClaimValue(SanteDBClaimTypes.Email);

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new InvalidOperationException("E-Mail TFA requires e-mail address registered");
                }
                else if (!email.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                {
                    email = "mailto:" + email;
                }

                string secret = null;
                try
                {
                    secret = _TfaCodeProvider.GenerateTfaCode(ci);
                }
                catch (ArgumentException)
                {
                    secret = _TfaSecretManager.StartTfaRegistration(ci, 6, Rfc4226Mode.HotpIncrementOnGenerate, AuthenticationContext.SystemPrincipal);
                    _TfaSecretManager.FinishTfaRegistration(ci, secret, AuthenticationContext.SystemPrincipal);
                    secret = _TfaCodeProvider.GenerateTfaCode(ci);
                }

                var templatemodel = new Dictionary<String, Object>()
                {
                    {"user",  user.Name },
                    { "code", secret },
                    { "principal", AuthenticationContext.Current.Principal }
                };

                try
                {
                    _NotificationService.SendTemplatedNotification(new[] { email }, s_TemplateName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, templatemodel, null, false);
                    var censoredemail = email.Split('@')[1];
                    return $"Code sent to ******@{censoredemail}";
                }
                catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
                {
                    throw new Exception("Error sending notification for tfa email.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot send notification to non-claims identity.");
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
