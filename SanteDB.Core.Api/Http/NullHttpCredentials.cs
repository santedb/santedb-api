using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Request credentials which are NULL
    /// </summary>
    public class NullHttpCredentials : RestRequestCredentials
    {
        /// <inheritdoc/>
        public NullHttpCredentials(IPrincipal principal) : base(principal)
        {
        }

        /// <inheritdoc/>
        public override void SetCredentials(HttpWebRequest webRequest)
        {
        }
    }
}
