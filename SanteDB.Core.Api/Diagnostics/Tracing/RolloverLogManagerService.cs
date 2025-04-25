/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Data.Backup;
using SanteDB.Core.Http.Compression;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SharpCompress.Compressors.Deflate;
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
    public class RolloverLogManagerService : ILogManagerService, IProvideBackupAssets, IRestoreBackupAssets
    {

        private readonly Guid LOG_FILE_ASSET_ID = Guid.Parse("D0B81E88-6626-448B-A92E-6C1537B52BAD");
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(RolloverLogManagerService));

        // Log retention app settting
        public const string RETAIN_LOG_FILE_SETTING = "log.retention";

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public string ServiceName => "Log File Manager";

        /// <inheritdoc/>
        public Guid[] AssetClassIdentifiers => new Guid[] { LOG_FILE_ASSET_ID };

        // Root path
        private readonly string m_rootPath;
        private readonly int m_maxLogFiles;
        private readonly IPolicyEnforcementService m_pepService;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RolloverLogManagerService(IConfigurationManager configurationManager, IPolicyEnforcementService pepService)
        {
            var logFileTracerConfig = configurationManager.GetSection<DiagnosticsConfigurationSection>().TraceWriter.FirstOrDefault(o => o.TraceWriter == typeof(RolloverTextWriterTraceWriter)) ??
                new TraceWriterConfiguration()
                {
                    InitializationData = "santedb.log"
                };
            if (!Path.IsPathRooted(logFileTracerConfig.InitializationData))
            {
                this.m_rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else
            {
                this.m_rootPath = Path.GetDirectoryName(logFileTracerConfig.InitializationData);
            }

            // Is the max log retention setting set?
            if(Int32.TryParse(configurationManager.GetAppSetting(RETAIN_LOG_FILE_SETTING), out var retainMax))
            {
                this.m_maxLogFiles = retainMax;
            }
            else
            {
                this.m_maxLogFiles = 30;
            }

            var retainLogFiles = this.GetLogFiles().OrderByDescending(o => o.CreationTime).Skip(this.m_maxLogFiles);
            foreach(var ret in retainLogFiles)
            {
                this.m_tracer.TraceInfo("Retaining old log file {0}...", ret.Name);
                try
                {
                    using (var outFile = File.Create(Path.ChangeExtension(ret.FullName, ".log.gz")))
                    {
                        using(var cs = new GZipStream(outFile, SharpCompress.Compressors.CompressionMode.Compress))
                        {
                            using (var inFile = ret.OpenRead())
                            {
                                inFile.CopyTo(cs);
                            }
                            ret.Delete();
                        }
                    }
                }
                catch(Exception e)
                {
                    this.m_tracer.TraceWarning("Could not retain old log file {0} - {1}", ret.Name, e.ToHumanReadableString());
                }
            }
            this.m_pepService = pepService;
        }

        /// <inheritdoc/>
        public void DeleteLogFile(string logId)
        {
            this.m_pepService?.Demand(PermissionPolicyIdentifiers.DeleteServiceLogs);
            File.Delete(Path.ChangeExtension(Path.Combine(this.m_rootPath, logId), "log"));
        }

        /// <inheritdoc/>
        public FileInfo GetLogFile(string logId)
        {
            this.m_pepService?.Demand(PermissionPolicyIdentifiers.ReadServiceLogs);
            return new FileInfo(Path.ChangeExtension(Path.Combine(this.m_rootPath, logId), "log"));
        }

        /// <inheritdoc/>
        public IEnumerable<FileInfo> GetLogFiles()
        {
            this.m_pepService?.Demand(PermissionPolicyIdentifiers.ReadServiceLogs);
            return Directory.GetFiles(this.m_rootPath, "*.log").Select(o => new FileInfo(o));
        }


        /// <inheritdoc/>
        public bool Restore(IBackupAsset backupAsset)
        {
            if (backupAsset == null)
            {
                throw new ArgumentNullException(nameof(backupAsset));
            }
            else if (backupAsset.AssetClassId != LOG_FILE_ASSET_ID)
            {
                throw new InvalidOperationException();
            }

            using (var fs = File.Create(Path.Combine(this.m_rootPath, backupAsset.Name)))
            {
                using (var astr = backupAsset.Open())
                {
                    astr.CopyTo(fs);
                    return true;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IBackupAsset> GetBackupAssets()
        {
            foreach (var itm in this.GetLogFiles())
            {
                yield return new FileBackupAsset(LOG_FILE_ASSET_ID, itm.Name, itm.FullName);
            }
        }
    }
}
