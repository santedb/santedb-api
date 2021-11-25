using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

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