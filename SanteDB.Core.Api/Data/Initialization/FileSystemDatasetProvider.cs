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
            var dataPath = Path.Combine(Path.GetDirectoryName(typeof(FileSystemDatasetProvider).Assembly.Location), "data");
            if (Directory.Exists(dataPath))
            {
                return Directory.GetFiles(dataPath, "*.dataset").Select(o =>
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
