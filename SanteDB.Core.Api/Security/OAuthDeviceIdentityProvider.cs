using SanteDB.Core.Http;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{
    public class OAuthDeviceIdentityProvider : IDeviceIdentityProviderService
    {
        readonly IRestClient _AuthClient;
        readonly IDeviceIdentityProviderService _LocalIdentityProvider;

        public OAuthDeviceIdentityProvider(IRestClient authClient, IDeviceIdentityProviderService localIdentityProvider)
        {
            _AuthClient = authClient;
            _LocalIdentityProvider = localIdentityProvider;
        }
        public string ServiceName => "OAuth Device Identity Provider";

        public event EventHandler<AuthenticatedEventArgs> Authenticated;
        public event EventHandler<AuthenticatingEventArgs> Authenticating;

        private bool CanPingServer()
        {
            try
            {
                _AuthClient.Invoke<object, object>("PING", "/", null, null);
                return true;
            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            {
                return false;
            }
        }

        public void AddClaim(string deviceName, IClaim claim, IPrincipal principal, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public IPrincipal Authenticate(string deviceName, string deviceSecret, AuthenticationMethod authMethod = AuthenticationMethod.Any)
        {
            throw new NotImplementedException();
        }

        public void ChangeSecret(string deviceName, string deviceSecret, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public IDeviceIdentity CreateIdentity(string deviceName, string secret, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IClaim> GetClaims(string deviceName)
        {
            throw new NotImplementedException();
        }

        public IDeviceIdentity GetIdentity(string deviceName)
        {
            throw new NotImplementedException();
        }

        public Guid GetSid(string deviceName)
        {
            throw new NotImplementedException();
        }

        public void RemoveClaim(string deviceName, string claimType, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public void SetLockout(string deviceName, bool lockoutState, IPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }
}
