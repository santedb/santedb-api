/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using System;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Indicates that the upstream realm has changed
    /// </summary>
    public class UpstreamRealmChangedEventArgs : EventArgs
    {

        /// <summary>
        /// Get the upstream realm settings
        /// </summary>
        public IUpstreamRealmSettings UpstreamRealmSettings { get; }

        /// <summary>
        /// Create new realm change event args
        /// </summary>
        public UpstreamRealmChangedEventArgs(IUpstreamRealmSettings upstreamRealmSettings)
        {
            this.UpstreamRealmSettings = upstreamRealmSettings;
        }
    }

    /// <summary>
    /// Represents an upstream enrolment management service
    /// </summary>
    public interface IUpstreamManagementService : IServiceImplementation
    {

        /// <summary>
        /// The realm settings are changing but not committed.
        /// </summary>
        event EventHandler<UpstreamRealmChangedEventArgs> RealmChanging;

        /// <summary>
        /// The realm settings have changed
        /// </summary>
        event EventHandler<UpstreamRealmChangedEventArgs> RealmChanged;

        /// <summary>
        /// Joins the specified <paramref name="targetRealm"/>
        /// </summary>
        /// <param name="targetRealm">The target realm to join</param>
        /// <param name="replaceExistingRegistration">True if this join request should replace the existing service</param>
        /// <param name="welcomeMessage">The welcome information the server wants the client to show the user</param>
        void Join(IUpstreamRealmSettings targetRealm, bool replaceExistingRegistration, out string welcomeMessage);

        /// <summary>
        /// Determines if the upstream has been configured
        /// </summary>
        /// <returns>True if the upstream is configured</returns>
        bool IsConfigured();

        /// <summary>
        /// Gets the upstream realm
        /// </summary>
        /// <returns></returns>
        IUpstreamRealmSettings GetSettings();

        /// <summary>
        /// Un-joins the upstream target realm
        /// </summary>
        void UnJoin();


    }
}
