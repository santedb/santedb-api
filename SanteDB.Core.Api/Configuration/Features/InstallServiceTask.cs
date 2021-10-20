using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A generic task which installs a service
    /// </summary>
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
        /// <param name="serviceType">The type of service</param>
        public InstallServiceTask(IFeature owner, Type serviceType, Func<bool> queryValidateFunc, params Type[] exclusiveFor)
        {
            this.m_serviceType = serviceType;
            this.m_exclusive = exclusiveFor;
            this.Feature = owner;
            this.m_queryValidateFunc = queryValidateFunc;
        }

        /// <summary>
        /// Get the description
        /// </summary>
        public string Description => $"Installs the {this.m_serviceType.Name} service in to the host context";

        /// <summary>
        /// The feature
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name => $"Install {this.m_serviceType.Name}";

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
            foreach (var t in this.m_exclusive)
            {
                service.RemoveAll(o => t.IsAssignableFrom(o.Type));
            }
            service.Add(new TypeReferenceConfiguration(this.m_serviceType));
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

            return !service.Any(o => o.Type == this.m_serviceType) && this.m_queryValidateFunc();
        }
    }
}