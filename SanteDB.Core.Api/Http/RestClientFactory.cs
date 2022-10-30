using SanteDB.Core.Configuration.Http;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Http.Description;
using SanteDB.Core.i18n;
using SanteDB.Core.Interop;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Rest client factory
    /// </summary>
    internal class RestClientFactory : IRestClientFactory
    {
        // Get the rest client configuration section
        private readonly RestClientConfigurationSection m_configuration;

        /// <summary>
        /// DI constructor
        /// </summary>
        public RestClientFactory(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<RestClientConfigurationSection>();
            if(this.m_configuration == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, typeof(RestClientConfigurationSection)));
            }
        }

        /// <inheritdoc/>
        public IRestClient CreateRestClient(IRestClientDescription description) => Activator.CreateInstance(this.m_configuration.RestClientType.Type, description) as IRestClient;

        /// <inheritdoc/>
        public IRestClient GetRestClientFor(ServiceEndpointType serviceEndpointType) => this.GetRestClientFor(serviceEndpointType.ToString());

        /// <inheritdoc/>
        public IRestClient GetRestClientFor(string clientName)
        {
            var configuration = this.m_configuration.Client.Find(o => o.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase));
            if(configuration == null)
            {
                throw new KeyNotFoundException(String.Format(ErrorMessages.REST_CLIENT_NOT_FOUND, clientName));
            }
            return this.CreateRestClient(configuration);
        }
    }
}
