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
 * Date: 2023-5-19
 */
using SanteDB.Core.Services;
using System;
using System.Linq;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic task which removes a service from the SanteDB configuration
    /// </summary>
    public class UnInstallServiceTask : IConfigurationTask
    {
        // Type of service
        private Type m_serviceType;

        private Func<bool> m_queryValidateFunc;

        /// <summary>
        /// Remove service task
        /// </summary>
        /// <param name="owner">The owner feature</param>
        /// <param name="serviceType">The type of service</param>
        /// <param name="queryValidateFunc">The callback to call to determine if the removal needs to occur</param>
        public UnInstallServiceTask(IFeature owner, Type serviceType, Func<bool> queryValidateFunc)
        {
            this.m_serviceType = serviceType;
            this.Feature = owner;
            this.m_queryValidateFunc = queryValidateFunc;
        }

        /// <inheritdoc/>
        public string Description => $"Removes the {this.m_serviceType.Name} service in to the host context";

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name => $"Remove {this.m_serviceType.Name}";

#pragma warning disable CS0067
        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var service = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;

            service.RemoveAll(o => o.Type == this.m_serviceType);
            return true;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            var service = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;

            return service.Any(o => o.Type == this.m_serviceType) && this.m_queryValidateFunc();
        }
    }
}