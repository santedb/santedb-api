/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using System;
using System.Security.Principal;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Authentication context
    /// </summary>
    public sealed class AuthenticationContext 
    {

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
                ApplicationServiceContext.Current?.GetService<IPolicyDecisionService>()?.ClearCache(principal);
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
        private static readonly IPrincipal s_system = new GenericPrincipal(new GenericIdentity("SYSTEM"), new string[] { "SYSTEM" });
        
        /// <summary>
        /// Anonymous identity
        /// </summary>
        private static readonly IPrincipal s_anonymous = new GenericPrincipal(new GenericIdentity("ANONYMOUS"), new string[] {  "ANONYMOUS" });

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
        public AuthenticationContext(IPrincipal principal)
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
                if(s_current == null)
                    lock(s_lockObject)
                        s_current = new AuthenticationContext(s_anonymous);
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
            return new WrappedContext(principal, AuthenticationContext.Current);
        }
    }
}
