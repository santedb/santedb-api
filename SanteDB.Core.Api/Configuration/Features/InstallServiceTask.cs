/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Services;
using System;
using System.Linq;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic task which installs a service
    /// </summary>
    /// <remarks>This generic <see cref="IConfigurationTask"/> implementation verifies that the current status of a service in the SanteDB configuration file,
    /// removes any conflicting services, and adds the provided service. This task saves re-writing the same repetitive procedure of installing and enabling
    /// services on a SanteDB iCDR service host</remarks>
    public class InstallServiceTask : IConfigurationTask
    {
        // Type of service
        private Type m_serviceType;

        private Type[] m_exclusive;

        private Func<bool> m_queryValidateFunc;

        /// <summary>
        /// Install service task
        /// </summary>
        /// <param name="exclusiveFor">True if the <paramref name="serviceType"/> should be the only of its type</param>
        /// <param name="owner">The owner feature</param>
        /// <param name="queryValidateFunc">The function callback to be used to determine if the task needs to be run</param>
        /// <param name="serviceType">The type of service which is being installed</param>
        public InstallServiceTask(IFeature owner, Type serviceType, Func<bool> queryValidateFunc, params Type[] exclusiveFor)
        {
            this.m_serviceType = serviceType;
            this.m_exclusive = exclusiveFor;
            this.Feature = owner;
            this.m_queryValidateFunc = queryValidateFunc;
        }

        /// <inheritdoc/>
        public string Description => $"Installs the {this.m_serviceType.Name} service in to the host context";

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name => $"Install {this.m_serviceType.Name}";

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var service = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            foreach (var t in this.m_exclusive)
            {
                service.RemoveAll(o => t.IsAssignableFrom(o.Type));
            }
            service.Add(new TypeReferenceConfiguration(this.m_serviceType));
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

            return !service.Any(o => o.Type == this.m_serviceType) && this.m_queryValidateFunc();
        }
    }
}