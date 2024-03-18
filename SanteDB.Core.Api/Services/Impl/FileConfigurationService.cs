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
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using SanteDB.Core.Data.Backup;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Provides a redirected configuration service which reads configuration information from a file
    /// </summary>
    /// <remarks>
    /// This configuration manager implementation  reads from the configuration file <c>santedb.config.xml</c> in the same directory
    /// as the installed iCDR instance. This file is create either manually (<see href="https://help.santesuite.org/operations/server-administration/host-configuration-file">as documented here</see>), or
    /// using the <see href="https://help.santesuite.org/operations/server-administration/configuration-tool">Configuration Tool</see>.
    /// </remarks>
    [ServiceProvider("Local File Configuration Manager")]
    public class FileConfigurationService : IConfigurationManager,
        IProvideBackupAssets,
        IRestoreBackupAssets,
        IRequestRestarts
    {

        // Asset ID
        private static readonly Guid CONFIGURATION_FILE_ASSET_ID = Guid.Parse("09379015-3823-40F1-B051-573E9009E849");

        // Configuration file name
        private readonly String m_configurationFileName;
        private readonly FileBackupAsset m_configurationFileBackupAsset;
        private readonly ConcurrentDictionary<String, ConnectionString> m_transientConnectionStrings = new ConcurrentDictionary<string, ConnectionString>();

        /// <inheritdoc/>
        public event EventHandler RestartRequested;

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "File-Based Configuration Manager";

        /// <summary>
        /// Get the configuration
        /// </summary>
        public SanteDBConfiguration Configuration { get; protected set; }

        /// <summary>
        /// True if the configuration is readonly
        /// </summary>
        public bool IsReadonly { get; }

        /// <summary>
        /// Get the asset class identifiers this supports
        /// </summary>
        public Guid[] AssetClassIdentifiers => new Guid[] { CONFIGURATION_FILE_ASSET_ID };


        /// <summary>
        /// Create new file confiugration service.
        /// </summary>
        public FileConfigurationService() : this(string.Empty, false)
        {

        }

        /// <summary>
        /// Get configuration service
        /// </summary>
        public FileConfigurationService(string configFile, bool isReadonly)
        {
            try
            {
                if (string.IsNullOrEmpty(configFile))
                {
                    configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "santedb.config.xml");
                }
                else if (!Path.IsPathRooted(configFile))
                {
                    configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), configFile);
                }

                this.IsReadonly = isReadonly;
                this.m_configurationFileName = configFile;
                this.m_configurationFileBackupAsset = new FileBackupAsset(CONFIGURATION_FILE_ASSET_ID, "config", configFile);
                this.Reload();
            }
            catch (Exception e)
            {
                Trace.TraceError("Error loading configuration: {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Get the provided section
        /// </summary>
        public T GetSection<T>() where T : IConfigurationSection
        {
            return Configuration.GetSection<T>();
        }

        /// <summary>
        /// Get the specified application setting
        /// </summary>
        public string GetAppSetting(string key)
        {
            // Use configuration setting 
            string retVal = null;
            try
            {
                retVal = Configuration.GetSection<ApplicationServiceContextConfigurationSection>()?.AppSettings.Find(o => o.Key == key)?.Value;
            }
            catch
            {
            }

            return retVal;

        }

        /// <summary>
        /// Get connection string
        /// </summary>
        public ConnectionString GetConnectionString(string key)
        {
            // Use configuration setting 
            ConnectionString retVal = null;
            try
            {
                retVal = Configuration.GetSection<DataConfigurationSection>()?.ConnectionString.Find(o => o.Name == key);
            }
            catch { }

            if (retVal == null)
            {
                this.m_transientConnectionStrings.TryGetValue(key, out retVal);
            }
            return retVal;
        }

        /// <inheritdoc/>
        public void SetTransientConnectionString(string key, ConnectionString connectionString)
        {
            if (Configuration.GetSection<DataConfigurationSection>()?.ConnectionString.Any(o => o.Name == key) == true)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DUPLICATE_OBJECT, key));
            }
            this.m_transientConnectionStrings.AddOrUpdate(key, connectionString, (k, o) => o);
        }

        /// <summary>
        /// Set an application setting
        /// </summary>
        public void SetAppSetting(string key, string value)
        {
            if (this.IsReadonly)
            {
                throw new InvalidOperationException(ErrorMessages.OBJECT_READONLY);
            }

            var appSettings = this.Configuration.GetSection<ApplicationServiceContextConfigurationSection>().AppSettings;
            appSettings.RemoveAll(o => o.Key == key);
            appSettings.Add(new AppSettingKeyValuePair(key, value));
            this.SaveConfiguration();
        }

        /// <summary>
        /// Reload configuration from disk
        /// </summary>
        public void Reload()
        {
            var backupFileName = Path.ChangeExtension(this.m_configurationFileName, ".bak.gz");
            try
            {
                using (var s = File.OpenRead(this.m_configurationFileName))
                {
                    Configuration = SanteDBConfiguration.Load(s);
                    // Create a backup of this file since we could successfully load it
                    s.Seek(0, SeekOrigin.Begin);
                    using (var backupStream = File.Create(backupFileName))
                    {
                        using (var gzStream = new GZipStream(backupStream, CompressionMode.Compress))
                        {
                            s.CopyTo(gzStream);
                            gzStream.Flush();
                        }
                    }
                }
            }
            catch (XmlException)
            {
                this.RestoreBackupFile(backupFileName);
            }
            catch (Exception e) when (e.InnerException is XmlException)
            {
                this.RestoreBackupFile(backupFileName);
            }
        }

        /// <summary>
        /// Restore the backup configuration file
        /// </summary>
        /// <param name="backupFileName"></param>
        private void RestoreBackupFile(string backupFileName)
        {
            if (File.Exists(backupFileName))
            {
                using (var backupFile = File.OpenRead(backupFileName))
                {
                    using (var gzStream = new GZipStream(backupFile, CompressionMode.Decompress))
                    {
                        using (var configFileStream = File.Create(this.m_configurationFileName))
                        {
                            gzStream.CopyTo(configFileStream);
                            configFileStream.Flush();
                            configFileStream.Seek(0, SeekOrigin.Begin);
                            Configuration = SanteDBConfiguration.Load(configFileStream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save configuration
        /// </summary>
        public void SaveConfiguration()
        {
            if (this.IsReadonly)
            {
                throw new InvalidOperationException(ErrorMessages.OBJECT_READONLY);
            }
            var pepService = ApplicationServiceContext.Current?.GetService<IPolicyEnforcementService>();
            pepService?.Demand(PermissionPolicyIdentifiers.AlterSystemConfiguration);

            using (var s = File.Create(this.m_configurationFileName))
            {
                this.Configuration.Save(s);
            }
            this.RestartRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public bool Restore(IBackupAsset backupAsset)
        {
            if (backupAsset.AssetClassId.Equals(CONFIGURATION_FILE_ASSET_ID))
            {
                using (var assetStream = backupAsset.Open())
                {
                    using (var configStream = File.Create(this.m_configurationFileName))
                    {
                        assetStream.CopyTo(configStream);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public IEnumerable<IBackupAsset> GetBackupAssets()
        {
            yield return this.m_configurationFileBackupAsset;
        }
    }
}
