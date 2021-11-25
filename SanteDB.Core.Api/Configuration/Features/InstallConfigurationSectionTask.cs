using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic configuration task which installs the provided configuration section into the configuration file
    /// </summary>
    /// <remarks>This class is used to wrap common, repetitive task whereby a configuration section needs to be validated and added
    /// to the configuration file provided.</remarks>
    public class InstallConfigurationSectionTask : IConfigurationTask
    {
        /// <summary>
        /// Section for configuration
        /// </summary>
        private IConfigurationSection m_section;

        /// <summary>
        /// Install configuration section task
        /// </summary>
        /// <param name="nameOfService">The name of the service that is being installed</param>
        /// <param name="owner">The owner feature of this task</param>
        /// <param name="section">The section which is to be installed</param>
        public InstallConfigurationSectionTask(IFeature owner, IConfigurationSection section, string nameOfService)
        {
            this.Feature = owner;
            this.m_section = section;
            this.Name = $"Update {nameOfService} Configuration";
            this.Description = $"Adds or updates the {section.GetType().Name} configuration section which controls {nameOfService}";
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
            configuration.AddSection(this.m_section);
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