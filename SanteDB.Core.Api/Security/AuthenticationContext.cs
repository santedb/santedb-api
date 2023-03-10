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
 * Date: 2023-3-10
 */
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Authentication context
    /// </summary>
    public sealed class AuthenticationContext
    {

        /// <summary>
        /// Identity for anonymous
        /// </summary>
        private sealed class SanteDBAnonymousIdentity : IIdentity
        {
            /// <inheritdoc/>
            public string AuthenticationType => "NONE";

            /// <inheritdoc/>
            public bool IsAuthenticated => false;

            /// <inheritdoc/>
            public string Name => "ANONYMOUS";
        }

        /// <summary>
        /// Anonymous principal
        /// </summary>
        private sealed class SanteDBAnonymousPrincipal : IPrincipal
        {

            private readonly SanteDBAnonymousIdentity m_identity = new SanteDBAnonymousIdentity();

            /// <inheritdoc/>
            public IIdentity Identity => this.m_identity;

            /// <inheritdoc/>
            public bool IsInRole(string role) => false;
        }

        /// <summary>
        /// Identity for anonymous
        /// </summary>
        private sealed class SanteDBSystemApplicationIdentity : IClaimsIdentity, IApplicationIdentity
        {
            private readonly IClaim[] m_claims = new IClaim[]
            {
                new SanteDBClaim(SanteDBClaimTypes.Name, "SYSTEM"),
                new SanteDBClaim(SanteDBClaimTypes.Actor, ActorTypeKeys.Application.ToString()),
                new SanteDBClaim(SanteDBClaimTypes.AuthenticationInstant, DateTimeOffset.Now.ToString("o")),
                new SanteDBClaim(SanteDBClaimTypes.AuthenticationMethod, "SYSTEM"),
                new SanteDBClaim(SanteDBClaimTypes.AuthenticationType, "SYSTEM"),
                new SanteDBClaim(SanteDBClaimTypes.LocalOnly, "true"),
                new SanteDBClaim(SanteDBClaimTypes.NameIdentifier, AuthenticationContext.SystemApplicationSid),
                new SanteDBClaim(SanteDBClaimTypes.SecurityId, AuthenticationContext.SystemApplicationSid),
                new SanteDBClaim(SanteDBClaimTypes.SanteDBApplicationIdentifierClaim, AuthenticationContext.SystemApplicationSid)
            };

            /// <inheritdoc/>
            public string AuthenticationType => "SYSTEM";

            /// <inheritdoc/>
            public bool IsAuthenticated => true;

            /// <inheritdoc/>
            public string Name => "SYSTEM";

            /// <inheritdoc/>
            public IEnumerable<IClaim> Claims => this.m_claims;

            /// <inheritdoc/>
            public IEnumerable<IClaim> FindAll(string claimType) => this.m_claims.Where(o => o.Type == claimType);

            /// <inheritdoc/>
            public IClaim FindFirst(string claimType) => this.m_claims.FirstOrDefault(o => o.Type == claimType);

        }

        /// <summary>
        /// Identity for system
        /// </summary>
        private sealed class SanteDBSystemUserIdentity : IClaimsIdentity
        {
            private readonly IClaim[] m_claims = new IClaim[]
            {
                new SanteDBClaim(SanteDBClaimTypes.Name, "SYSTEM"),
                new SanteDBClaim(SanteDBClaimTypes.Actor, ActorTypeKeys.System.ToString()),
                new SanteDBClaim(SanteDBClaimTypes.AuthenticationInstant, DateTimeOffset.Now.ToString("o")),
                new SanteDBClaim(SanteDBClaimTypes.AuthenticationMethod, "SYSTEM"),
                new SanteDBClaim(SanteDBClaimTypes.AuthenticationType, "SYSTEM"),
                new SanteDBClaim(SanteDBClaimTypes.LocalOnly, "true"),
                new SanteDBClaim(SanteDBClaimTypes.NameIdentifier, AuthenticationContext.SystemUserSid),
                new SanteDBClaim(SanteDBClaimTypes.SecurityId, AuthenticationContext.SystemUserSid)
            };

            /// <inheritdoc/>
            public string AuthenticationType => "SYSTEM";

            /// <inheritdoc/>
            public bool IsAuthenticated => true;

            /// <inheritdoc/>
            public string Name => "SYSTEM";

            /// <inheritdoc/>
            public IEnumerable<IClaim> Claims => this.m_claims;

            /// <inheritdoc/>
            public IEnumerable<IClaim> FindAll(string claimType) => this.m_claims.Where(o => o.Type == claimType);

            /// <inheritdoc/>
            public IClaim FindFirst(string claimType) => this.m_claims.FirstOrDefault(o => o.Type == claimType);
        
        }

        /// <summary>
        /// Anonymous principal
        /// </summary>
        private sealed class SanteDBSystemPrincipal : IClaimsPrincipal
        {
            private readonly IClaimsIdentity[] m_identities = new IClaimsIdentity[]
            {
                new SanteDBSystemUserIdentity(),
                new SanteDBSystemApplicationIdentity()
            };

            /// <inheritdoc/>
            public IIdentity Identity => this.m_identities[0];

            /// <inheritdoc/>
            public IEnumerable<IClaim> Claims => this.m_identities.SelectMany(o => o.Claims);

            /// <inheritdoc/>
            public IClaimsIdentity[] Identities => this.m_identities;

            /// <inheritdoc/>
            public void AddIdentity(IIdentity identity)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public IEnumerable<IClaim> FindAll(string claimType) => this.Claims.Where(o => o.Type == claimType);

            /// <inheritdoc/>
            public IClaim FindFirst(string claimType) => this.Claims.FirstOrDefault(o => o.Type == claimType);

            /// <inheritdoc/>
            public bool HasClaim(Func<IClaim, bool> predicate) => this.Claims.Any(predicate);

            /// <inheritdoc/>
            public bool IsInRole(string role) => false;

            /// <inheritdoc/>
            public bool TryGetClaimValue(string claimType, out string value)
            {
                var claim = this.Claims.FirstOrDefault(o => o.Type == claimType);
                value = claim?.Value;
                return value != null;
            }
        }

        /// <summary>
        /// Represents a authentication context that sets the restore context on disposal
        /// </summary>
        private class WrappedContext : IDisposable
        {

            /// <summary>
            /// The previous context
            /// </summary>
            public AuthenticationContext RestoreContext { get; }

            /// <summary>
            /// Create new wrapped authentication context
            /// </summary>
            public WrappedContext(IPrincipal principal, AuthenticationContext restore)
            {
                this.RestoreContext = restore;
                AuthenticationContext.Current = new AuthenticationContext(principal);

            }

            /// <summary>
            /// Restores the context
            /// </summary>
            public void Dispose()
            {
                AuthenticationContext.Current = this.RestoreContext;
            }
        }

        /// <summary>
        /// SYSTEM user's SID
        /// </summary>
        public const String SystemUserSid = "fadca076-3690-4a6e-af9e-f1cd68e8c7e8";

        /// <summary>
        /// ANONYMOUS user's SID
        /// </summary>
        public const String AnonymousUserSid = "C96859F0-043C-4480-8DAB-F69D6E86696C";

        /// <summary>
        /// SYSTEM application's SID
        /// </summary>
        public const String SystemApplicationSid = "4c5b9f8d-49f4-4101-9662-4270895224b2";

        /// <summary>
        /// System identity
        /// </summary>
        private static readonly IPrincipal s_system = new SanteDBSystemPrincipal(); //new GenericPrincipal(new GenericIdentity("SYSTEM"), new string[] { "SYSTEM" });

        /// <summary>
        /// Anonymous identity
        /// </summary>
        private static readonly IPrincipal s_anonymous = new SanteDBAnonymousPrincipal();

        /// <summary>
        /// Gets the anonymous principal
        /// </summary>
        public static IPrincipal AnonymousPrincipal
        {
            get
            {
                return s_anonymous;
            }
        }

        /// <summary>
        /// Get the system principal
        /// </summary>
        public static IPrincipal SystemPrincipal
        {
            get
            {
                return s_system;
            }
        }

        /// <summary>
        /// Current context in the request pipeline
        /// </summary>
        [ThreadStatic]
        private static AuthenticationContext s_current;

        /// <summary>
        /// Locking
        /// </summary>
        private static Object s_lockObject = new object();

        /// <summary>
        /// The principal
        /// </summary>
        private IPrincipal m_principal;

        /// <summary>
        /// Previous context
        /// </summary>
        private AuthenticationContext m_previous;

        /// <summary>
        /// Creates a new context keeping track of previous
        /// </summary>
        private AuthenticationContext(IPrincipal principal, AuthenticationContext previous)
        {
            this.m_previous = previous;
            this.m_principal = principal;
        }

        /// <summary>
        /// Creates a new instance of the authentication context
        /// </summary>
        private AuthenticationContext(IPrincipal principal)
        {
            this.m_principal = principal;
        }


        /// <summary>
        /// Gets or sets the current context
        /// </summary>
        public static AuthenticationContext Current
        {
            get
            {
                if (s_current == null)
                {
                    lock (s_lockObject)
                    {
                        s_current = new AuthenticationContext(s_anonymous);
                    }
                }

                return s_current;
            }
            internal set { s_current = value; }
        }

        /// <summary>
        /// Gets the principal 
        /// </summary>
        public IPrincipal Principal
        {
            get
            {
                return this.m_principal;
            }
        }

        /// <summary>
        /// Get the parent context
        /// </summary>
        public AuthenticationContext Parent => this.m_previous;
        /// <summary>
        /// Enter the system context
        /// </summary>
        public static IDisposable EnterSystemContext()
        {
            // TODO: Validate the caller can enter the system context
            return new WrappedContext(AuthenticationContext.SystemPrincipal, AuthenticationContext.Current);
        }

        /// <summary>
        /// Enter a wrapped context
        /// </summary>
        public static IDisposable EnterContext(IPrincipal principal)
        {
            try
            {
                return new WrappedContext(principal, AuthenticationContext.Current);
            }
            finally
            {
                ApplicationServiceContext.Current?.GetService<IPolicyDecisionService>()?.ClearCache(principal);
            }
        }
    }
}
