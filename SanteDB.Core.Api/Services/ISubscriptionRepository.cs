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
using SanteDB.Core.Model.Subscription;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a repository which maintains subscription definitions
    /// </summary>
    /// <remarks>
    /// <para>This service is used to maintain instances of <see cref="SubscriptionDefinition"/> which 
    /// contain SQL (or other query grammar) to allow dCDR instances to easily query data on the HDSI interface
    /// using the <c>_subscription</c> parameter. The HDSI maps the subscription ID with the local data provider
    /// and then executes the appopriate query against the persistence layer to ensure fast synchronization of
    /// new data.</para>
    /// </remarks>
    [System.ComponentModel.Description("dCDR Subscription Definition Provider")]
    public interface ISubscriptionRepository : IRepositoryService<SubscriptionDefinition>
    {
    }
}
