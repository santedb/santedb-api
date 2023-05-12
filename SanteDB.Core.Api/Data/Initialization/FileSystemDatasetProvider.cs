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
 * Date: 2023-3-10
 */
using SanteDB.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// An implementation of the <see cref="IDatasetProvider"/> which uses the data directory
    /// </summary>
    public class FileSystemDatasetProvider : IDatasetProvider
    {

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(FileSystemDatasetProvider));

        /// <summary>
        /// Get all datasets
        /// </summary>
        public IEnumerable<Dataset> GetDatasets()
        {
            var dataPath = Path.Combine(Path.GetDirectoryName(typeof(FileSystemDatasetProvider).Assembly.Location), "Data");
            if (!Directory.Exists(dataPath)) // HACK: Might be on linux or have a lower case data file
            {
                dataPath = Path.Combine(Path.GetDirectoryName(typeof(FileSystemDatasetProvider).Assembly.Location), "data");
            }
            if (Directory.Exists(dataPath))
            {
                return Directory.GetFiles(dataPath, "*.dataset").OrderBy(o => o).Select(o =>
                {
                    this.m_tracer.TraceVerbose("Loading {0}...", Path.GetFileName(o));
                    try
                    {
                        using (var fs = File.OpenRead(o))
                        {
                            return Dataset.Load(fs);
                        }
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceError("Could not load {0} - {1}", o, e);
                        return null;
                    }
                }).OfType<Dataset>();
            }
            else
            {
                this.m_tracer.TraceWarning($"Directory {dataPath} does not exist! No file application of datasets will be performed");
            }
            return null;
        }
    }
}
