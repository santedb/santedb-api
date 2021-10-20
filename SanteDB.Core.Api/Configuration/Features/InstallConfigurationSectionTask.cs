using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Install configuration section into the configuraion file
    /// </summary>
    public class InstallConfigurationSectionTask : IConfigurationTask
    {
        /// <summary>
        /// Section for configuration
        /// </summary>
        private IConfigurationSection m_section;

        /// <summary>
        /// Install configuration section task
        /// </summary>
        public InstallConfigurationSectionTask(IFeature owner, IConfigurationSection section, string nameOfService)
        {
            this.Feature = owner;
            this.m_section = section;
            this.Name = $"Update {nameOfService} Configuration";
            this.Description = $"Adds or updates the {section.GetType().Name} configuration section which controls {nameOfService}";
        }

        /// <summary>
        /// Get the description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Get the owner
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Get the name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the configuration
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            configuration.RemoveSection(this.m_section.GetType());
            configuration.AddSection(this.m_section);
            return true;
        }

        /// <summary>
        /// Rollback configuration
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <summary>
        /// Verify status
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }
}