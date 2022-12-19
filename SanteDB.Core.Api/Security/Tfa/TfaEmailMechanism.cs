﻿using SanteDB.Core.Notifications;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Tfa
{
    public class TfaEmailMechanism : ITfaMechanism
    {
        private static readonly Guid s_MechanismId = Guid.Parse("D919457D-E015-435C-BD35-42E425E2C60C");
        private static readonly string s_TemplateName = "tfa.email";

        readonly INotificationService _NotificationService;
        readonly ITfaCodeProvider _TfaCodeProvider;

        public TfaEmailMechanism(INotificationService notificationService, ITfaCodeProvider tfaCodeProvider)
        {
            _NotificationService = notificationService;
            _TfaCodeProvider = tfaCodeProvider;
        }

        public Guid Id => s_MechanismId;

        public string Name => "org.santedb.tfa.email";

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

                var secret = _TfaCodeProvider.GenerateTfaCode(ci);

                var templatemodel = new
                {
                    user,
                    tfa = secret,
                    secret,
                    code = secret,
                    principal = AuthenticationContext.Current.Principal
                };

                try
                {
                    _NotificationService.SendTemplatedNotification(new[] { email }, s_TemplateName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, templatemodel, null, false);
                    var censoredemail = email.Split(new[] { "@" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    return $"Code sent to ******@{censoredemail}";
                }
                catch(Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
                {
                    throw new Exception("Error sending notification for tfa email.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot send notification to non-claims identity.");
            }
        }

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
