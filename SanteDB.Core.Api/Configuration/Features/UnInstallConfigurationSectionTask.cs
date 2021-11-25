using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

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

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

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