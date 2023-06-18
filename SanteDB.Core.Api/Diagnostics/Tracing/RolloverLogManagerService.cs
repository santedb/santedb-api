/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.i18n;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SanteDB.Core.Diagnostics.Tracing
{
    /// <summary>
    /// A log file manager service which manages rollover logs
    /// </summary>
    public class RolloverLogManagerService : ILogManagerService
    {
        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public string ServiceName => "Log File Manager";

        // Root path
        private readonly string m_rootPath;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RolloverLogManagerService(IConfigurationManager configurationManager)
        {
            var logFileTracerConfig = configurationManager.GetSection<DiagnosticsConfigurationSection>().TraceWriter.FirstOrDefault(o => o.TraceWriter == typeof(RolloverTextWriterTraceWriter));
            if (logFileTracerConfig == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, typeof(RolloverTextWriterTraceWriter)));
            }
            if (!Path.IsPathRooted(logFileTracerConfig.InitializationData))
            {
                this.m_rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else
            {
                this.m_rootPath = Path.GetDirectoryName(logFileTracerConfig.InitializationData);
            }
        }

        /// <inheritdoc/>
        public void DeleteLogFile(string logId)
        {
            File.Delete(Path.ChangeExtension(Path.Combine(this.m_rootPath, logId), "log"));
        }

        /// <inheritdoc/>
        public FileInfo GetLogFile(string logId)
        {
            return new FileInfo(Path.ChangeExtension(Path.Combine(this.m_rootPath, logId), "log"));
        }

        /// <inheritdoc/>
        public IEnumerable<FileInfo> GetLogFiles()
        {
            return Directory.GetFiles(this.m_rootPath, "*.log").Select(o => new FileInfo(o));
        }
    }
}
