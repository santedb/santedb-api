using SanteDB.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// The performance monitor class
    /// </summary>
    public class DiagnosticsProbeManager
    {

        /// <summary>
        /// Counters
        /// </summary>
        private IEnumerable<IDiagnosticsProbe> m_probes;

        // Lock object
        private static object m_lockObject = new object();

        /// <summary>
        /// The current performance monitor
        /// </summary>
        private static DiagnosticsProbeManager m_current;

        /// <summary>
        /// Creates a new performance monitor
        /// </summary>
        private DiagnosticsProbeManager()
        {
            this.m_probes = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes()
                .Where(t => typeof(IDiagnosticsProbe).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) && !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().IsInterface && t.GetTypeInfo().DeclaredConstructors.Any(o=>o.GetParameters().Length == 0))
                .Select(t => Activator.CreateInstance(t) as IDiagnosticsProbe)
                .ToArray();
        }

        /// <summary>
        /// Gets the current performance monitor
        /// </summary>
        public static DiagnosticsProbeManager Current
        {
            get
            {
                if (m_current == null)
                    lock (m_lockObject)
                        if (m_current == null)
                            m_current = new DiagnosticsProbeManager();
                return m_current;
            }
        }

        /// <summary>
        /// Get the performance counter
        /// </summary>
        /// <param name="uuid">The uuid of the performance counter to retrieve</param>
        /// <returns>The identified performance counter if present</returns>
        public IDiagnosticsProbe Get(Guid uuid)
        {
            return this.m_probes.FirstOrDefault(o => o.Uuid == uuid);
        }

        /// <summary>
        /// Find the specified performance counters
        /// </summary>
        public IEnumerable<IDiagnosticsProbe> Find(Func<IDiagnosticsProbe, bool> query, int offset, int? count, out int totalResults)
        {
            var matches = this.m_probes.Where(query);
            totalResults = matches.Count();
            return matches.Skip(offset).Take(count ?? 100);
        }
    }
}
