using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Authentication;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Password is expired and must be changed
    /// </summary>
    public class PasswordExpiredException : AuthenticationException
    {
        /// <summary>
        /// Create a new password expiration exception
        /// </summary>
        public PasswordExpiredException(IPrincipal resetPasswordPrincipal) : base(ErrorMessages.PASSWORD_EXPIRED)
        {
            this.Principal = resetPasswordPrincipal;
        }

        /// <summary>
        /// Gets an authenticated principal which is allowed to reset its password
        /// </summary>
        public IPrincipal Principal { get; }
    }
}
