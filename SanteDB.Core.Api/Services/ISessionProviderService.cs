/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Security.Principal;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Event arguments for session establishment
    /// </summary>
    public class SessionEstablishedEventArgs : EventArgs
    {

        /// <summary>
        /// Gets the principal which was used to establish sessions
        /// </summary>
        public IPrincipal Principal { get; private set; }

        /// <summary>
        /// Gets the established session
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets whether session established was successful
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Elevated sesison
        /// </summary>
        public bool Elevated { get; private set; }

        /// <summary>
        /// Purpose of the session
        /// </summary>
        public String Purpose { get; private set; }

        /// <summary>
        /// Policies requested for the session
        /// </summary>
        public String[] Policies { get; private set; }

        /// <summary>
        /// Creates a new session establishement args
        /// </summary>
        public SessionEstablishedEventArgs(IPrincipal principal, ISession session, bool success, bool elevated, String purpose, String[] policies)
        {
            this.Success = success;
            this.Session = session;
            this.Principal = principal;
            this.Elevated = elevated;
            this.Purpose = purpose;
            this.Policies = policies;
        }
    }
    
    /// <summary>
    /// Represents a service which is responsible for the storage and retrieval of sessions
    /// </summary>
    public interface ISessionProviderService : IServiceImplementation
    {
        
        /// <summary>
        /// Fired when the session provider service has established
        /// </summary>
        event EventHandler<SessionEstablishedEventArgs> Established;

        /// <summary>
        /// Fired when the session provider service has ended by the user's decision
        /// </summary>
        event EventHandler<SessionEstablishedEventArgs> Abandoned;

        /// <summary>
        /// Establishes a session for the specified principal
        /// </summary>
        /// <param name="principal">The principal for which the session is to be established</param>
        /// <param name="remoteEp">The remote endpoint</param>
        /// <returns>The session information that was established</returns>
        /// <param name="purpose">The purpose of the session</param>
        /// <param name="scope">The scope of the session (policies)</param>
        /// <param name="isOverride">True if the session is an override session</param>
        /// <param name="lang">The language for the session</param>
        ISession Establish(IPrincipal principal, String remoteEp, bool isOverride, String purpose, String[] scope, String lang);

        /// <summary>
        /// Authenticates the session identifier as evidence of session
        /// </summary>
        /// <param name="sessionToken">The session identiifer to be authenticated</param>
        /// <param name="allowExpired">When true, allows the retrieval of expired session</param>
        /// <returns>The authenticated session from the session provider</returns>
        ISession Get(byte[] sessionToken, bool allowExpired = false);

        /// <summary>
        /// Extend the session with the specified refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token that will extend the session</param>
        /// <returns>The extended session</returns>
        ISession Extend(byte[] refreshToken);

        /// <summary>
        /// Abandons the session 
        /// </summary>
        void Abandon(ISession session);

       
    }
}
