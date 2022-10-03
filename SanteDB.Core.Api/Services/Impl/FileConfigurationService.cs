/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
    public class FileConfigurationService : IConfigurationManager
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "File-Based Configuration Manager";

        /// <summary>
        /// Get the configuration
        /// </summary>
        public SanteDBConfiguration Configuration { get; set; }

        /// <summary>
        /// Create new file confiugration service.
        /// </summary>
        public FileConfigurationService() : this(string.Empty)
        {

        }

        /// <summary>
        /// Get configuration service
        /// </summary>
        public FileConfigurationService(string configFile)
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

                using (var s = File.OpenRead(configFile))
                {
                    Configuration = SanteDBConfiguration.Load(s);
                }
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

            return retVal;
        }

        /// <summary>
        /// Set an application setting
        /// </summary>
        public void SetAppSetting(string key, string value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reload configuration from disk
        /// </summary>
        public void Reload()
        {
            throw new NotSupportedException();
        }
    }
}
