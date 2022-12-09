/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Security.Principal;

namespace SanteDB.Security.Tfa.Email
{
    /// <summary>
    /// Represents a TFA mechanism which can send/receive TFA requests via e-mail
    /// </summary>
    public class DefaultTfaMechanism : ITfaMechanism
    {
        private readonly ITfaCodeProvider m_tfaCodeProvider;
        private readonly INotificationService m_notificationService;

        /// <summary>
        /// TFA Mechanism via e-mail
        /// </summary>
        public DefaultTfaMechanism(ITfaCodeProvider tfaCodeProvider, INotificationService notificationService)
        {
            this.m_tfaCodeProvider = tfaCodeProvider;
            this.m_notificationService = notificationService;
        }

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultTfaMechanism));

        /// <summary>
        /// Get the identifier for the challenge
        /// </summary>
        public Guid Id
        {
            get
            {
                return Guid.Parse("D919457D-E015-435C-BD35-42E425E2C60C");
            }
        }

        /// <summary>
        /// Gets the name of the mechanism
        /// </summary>
        public string Name
        {
            get
            {
                return "org.santedb.tfa";
            }
        }

        /// <summary>
        /// Send the mechanism
        /// </summary>
        public string Send(IIdentity user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // We want to send the data in which language?
            String language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            // Generate a TFA secret and add it as a claim on the user
            

            if (user is IClaimsIdentity ci)
            {
                var secret = this.m_tfaCodeProvider.GenerateTfaCode(user);

                // Get user's e-mail
                var email = ci.FindFirst(SanteDBClaimTypes.Email)?.Value;
                if (email == null)
                {
                    throw new InvalidOperationException("E-Mail TFA requires e-mail address registered");
                }

                // Send
                var templatemodel = new
                {
                    user,
                    tfa = secret,
                    principal = AuthenticationContext.Current.Principal
                };

                try
                {
                    ApplicationServiceContext.Current.GetService<INotificationService>().SendTemplatedNotification(new[] { email }, "tfa.email", language, templatemodel, null, false, null);
                    var censoredEmail = email.Split('@')[1];
                    return $"Code sent to *****@{censoredEmail}";
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceEvent(EventLevel.Error, $"Error sending TFA secret: {e.Message}\r\n{e.ToString()}");
                    throw new Exception($"Error dispatching OTP TFA code", e);
                }
            }
            else
            {
                throw new InvalidOperationException("TFA E-mail not compatible with this mechanism");
            }
        }

        /// <summary>
        /// Validate the secret
        /// </summary>
        public bool Validate(IIdentity user, String tfaSecret)
        {
            if (user is IClaimsIdentity ci)
            {
                return false;
                //return ci.FindFirst(SanteDBClaimTypes.SanteDBOTAuthCode)?.Value == this.m_hashingProvider.ComputeHash(tfaSecret);
            }
            else
            {
                // TODO: When a non-CI is provided
                return false;
            }
        }
    }
}