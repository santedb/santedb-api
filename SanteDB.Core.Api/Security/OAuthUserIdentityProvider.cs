using Newtonsoft.Json;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Http;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{
    public class OAuthUserIdentityProvider : IIdentityProviderService
    {
        readonly IRestClient _AuthClient;
        readonly IIdentityProviderService _LocalIdentityProvider;
        readonly Tracer _Tracer = new Tracer(nameof(OAuthUserIdentityProvider));

        private class TokenRequest
        {
            [JsonProperty("grant_type")]
            public string GrantType;
            [JsonProperty("username")]
            public string Username;
            [JsonProperty("password")]
            public string Password;
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken;
            [JsonProperty("id_token")]
            public string IdToken;
            [JsonProperty("token_type")]
            public string TokenType;
            [JsonProperty("expires_in")]
            public string ExpiresIn;
        }


        public OAuthUserIdentityProvider(IRestClient authClient, IIdentityProviderService localIdentityProvider)
        {
            _AuthClient = authClient;
            _LocalIdentityProvider = localIdentityProvider;
        }

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

        public string ServiceName => "OAuth User Identity Provider";

        public event EventHandler<AuthenticatingEventArgs> Authenticating;
        public event EventHandler<AuthenticatedEventArgs> Authenticated;

        public void AddClaim(string userName, IClaim claim, IPrincipal principal, TimeSpan? expiry = null)
        {
            _LocalIdentityProvider.AddClaim(userName, claim, principal, expiry);
        }

        private void TryUpdateLocalFromRemote(string userName, string password, string tfaSecret = null)
        {
            try
            {
                var request = new TokenRequest
                {
                    GrantType = "password",
                    Username = userName,
                    Password = password
                };

                var response = _AuthClient.Post<TokenRequest, TokenResponse>("oauth2_token", request);

                if (null != response && !string.IsNullOrEmpty(response.AccessToken))
                {
                    UpdateLocalRepository(userName, password, response.AccessToken);
                }

            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            {
                _Tracer.TraceError("Exception when calling upstream server. {0}", ex);
            }
        }

        public IPrincipal Authenticate(string userName, string password)
        {
            TryUpdateLocalFromRemote(userName, password);

            return _LocalIdentityProvider.Authenticate(userName, password);
        }

        public IPrincipal Authenticate(string userName, string password, string tfaSecret)
        {
            TryUpdateLocalFromRemote(userName, password, tfaSecret);

            return _LocalIdentityProvider.Authenticate(userName, password, tfaSecret);
        }



        private void UpdateLocalRepository(string userName, string password, string accessToken)
        {

        }

        public void ChangePassword(string userName, string newPassword, IPrincipal principal)
        {
            _LocalIdentityProvider.ChangePassword(userName, newPassword, principal);
        }

        public IIdentity CreateIdentity(string userName, string password, IPrincipal principal)
        {
            return _LocalIdentityProvider.CreateIdentity(userName, password, principal);
        }

        public void DeleteIdentity(string userName, IPrincipal principal)
        {
            _LocalIdentityProvider.DeleteIdentity(userName, principal);
        }

        public IEnumerable<IClaim> GetClaims(string userName)
        {
            return _LocalIdentityProvider.GetClaims(userName);
        }

        public IIdentity GetIdentity(string userName)
        {
            return _LocalIdentityProvider.GetIdentity(userName);
        }

        public IIdentity GetIdentity(Guid sid)
        {
            return _LocalIdentityProvider.GetIdentity(sid);
        }

        public Guid GetSid(string name)
        {
            return _LocalIdentityProvider.GetSid(name);
        }

        public void RemoveClaim(string userName, string claimType, IPrincipal principal)
        {
            _LocalIdentityProvider.RemoveClaim(userName, claimType, principal);
        }

        public void SetLockout(string userName, bool lockout, IPrincipal principal)
        {
            _LocalIdentityProvider.SetLockout(userName, lockout, principal);
        }
    }
}
