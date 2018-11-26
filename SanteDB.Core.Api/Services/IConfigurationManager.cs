using SanteDB.DisconnectedClient.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents a connection string
    /// </summary>
    public sealed class ConnectionStringInfo
    {

        /// <summary>
        /// Gets the specified connection string
        /// </summary>
        public ConnectionStringInfo(string provider, string connectionString)
        {
            this.ProviderName = provider;
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the provider name
        /// </summary>
        public String ProviderName { get; private set; }

        /// <summary>
        /// Gets the specified connection string
        /// </summary>
        public String ConnectionString { get; set; }

    }

    /// <summary>
    /// Represents a configuration manager service
    /// </summary>
    public interface IConfigurationManager
    {

        /// <summary>
        /// Get the specified configuration section
        /// </summary>
        T GetSection<T>();

        /// <summary>
        /// Gets the specified application setting
        /// </summary>
        String GetAppSetting(String key);

        /// <summary>
        /// Get the specified connection string
        /// </summary>
        ConnectionStringInfo GetConnectionString(String key);

        /// <summary>
        /// Get the configuration object
        /// </summary>
        SanteDBConfiguration Configuration { get; }

        /// <summary>
        /// Set the specified application setting
        /// </summary>
        void SetAppSetting(string key, string value);

        /// <summary>
        /// Forces the configuration manager to reload
        /// </summary>
        void Reload();
    }
}
