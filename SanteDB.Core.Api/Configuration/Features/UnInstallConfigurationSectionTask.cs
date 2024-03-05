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
using SanteDB.Core.Services;
using System;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic task which removes a configuration section from the configuration file
    /// </summary>
    public class UnInstallConfigurationSectionTask : IConfigurationTask
    {
        /// <summary>
        /// Section for configuration
        /// </summary>
        private IConfigurationSection m_section;

        /// <summary>
        /// Remove configuration section task
        /// </summary>
        /// <param name="section">The section which is removed by this task</param>
        /// <param name="owner">The owner feature of this task</param>
        /// <param name="nameOfService">The name of the service which this feature configures</param>
        public UnInstallConfigurationSectionTask(IFeature owner, IConfigurationSection section, string nameOfService)
        {
            this.Feature = owner;
            this.m_section = section;
            this.Name = $"Remove {nameOfService} Configuration";
            this.Description = $"Removes the {section.GetType().Name} configuration section which controls {nameOfService}";
        }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name { get; }

#pragma warning disable CS0067
        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            configuration.RemoveSection(this.m_section.GetType());
            return true;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }
}