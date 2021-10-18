using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic task which removes a service
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

        /// <summary>
        /// Get the description
        /// </summary>
        public string Description => $"Removes the {this.m_serviceType.Name} service in to the host context";

        /// <summary>
        /// The feature
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name => $"Remove {this.m_serviceType.Name}";

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the install
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var service = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;

            service.RemoveAll(o => o.Type == this.m_serviceType);
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
        /// Verify this can be installed
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            var service = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;

            return service.Any(o => o.Type == this.m_serviceType) && this.m_queryValidateFunc();
        }
    }
}