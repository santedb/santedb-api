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
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A marker interface which indicates that this service is an local service for wrapper <typeparamref name="TLocalService"/>
    /// </summary>
    /// <typeparam name="TLocalService">The local service service that this upstream wrapper </typeparam>
    /// <remarks>There are contexts where SanteDB operates where the a service provider must rely on a local fallback. For example, 
    /// the <see cref="IIdentityProviderService"/> which operates in a synchronization mode must attempt to contact the upstream
    /// prior to contacting the local identity provider service. This interface allows these services to identify and differentiate between 
    /// a service provider which provides local funcitonality only. The <see cref="LocalProvider"/> property is a pointer to the service itself, 
    /// and is used to overcome the fact that <c>ILocalService&lt;TLocalService> : TLocalService</c> is not permitted.</remarks>
    /// <seealso cref="IUpstreamServiceProvider{TUpstreamService}"/>
    public interface ILocalServiceProvider<TLocalService>
    {

        /// <summary>
        /// Gets the service which provides explicitly local access
        /// </summary>
        TLocalService LocalProvider { get; }

    }

    /// <summary>
    /// Identifies an implementation as strictly providing upstream source for the information
    /// </summary>
    /// <typeparam name="TUpstreamService">The type of upstream service wrapped</typeparam>
    /// <seealso cref="ILocalServiceProvider{TLocalService}"/>
    public interface IUpstreamServiceProvider<TUpstreamService>
    {

        /// <summary>
        /// Gets the upstream provider
        /// </summary>
        TUpstreamService UpstreamProvider { get; }
    }

}
