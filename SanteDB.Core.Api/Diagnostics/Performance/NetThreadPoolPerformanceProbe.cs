using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SanteDB.Core.Diagnostics.Performance
{
    /// <summary>
    /// Represents a thread pool performance counter
    /// </summary>
    public class NetThreadPoolPerformanceProbe : ICompositeDiagnosticsProbe
    {
        // Performance counters
        private readonly IDiagnosticsProbe[] m_performanceCounters;

        /// <summary>
        /// Generic performance counter
        /// </summary>
        private class NetIoPooledWorkersProbe : DiagnosticsProbeBase<int>
        {
            /// <summary>
            /// .NET pool workers counter
            /// </summary>
            public NetIoPooledWorkersProbe() : base(".NET I/O Threads", "Shows the number of .NET I/O threads in the pool")
            {
            }

            /// <summary>
            /// Gets the identifier for the pool
            /// </summary>
            public override Guid Uuid => PerformanceConstants.NetIoThreads;

            /// <summary>
            /// Gets the value
            /// </summary>
            public override int Value
            {
                get
                {
                    ThreadPool.GetMaxThreads(out _, out var worker);
                    ThreadPool.GetAvailableThreads(out _, out var available);
                    return worker - available;
                }
            }

            /// <summary>
            /// Units for the probe
            /// </summary>
            public override String Unit => null;
        }

        /// <summary>
        /// Generic performance counter
        /// </summary>
        private class NetPooledWorkersProbe : DiagnosticsProbeBase<int>
        {
            /// <summary>
            /// .NET pool workers counter
            /// </summary>
            public NetPooledWorkersProbe() : base(".NET Pool", "Shows the number of busy worker threads in the .NET worker pool")
            {
            }

            /// <summary>
            /// Gets the identifier for the pool
            /// </summary>
            public override Guid Uuid => PerformanceConstants.NetPoolWorkerCounter;

            /// <summary>
            /// Gets the value
            /// </summary>
            public override int Value
            {
                get
                {
                    ThreadPool.GetMaxThreads(out var worker, out _);
                    ThreadPool.GetAvailableThreads(out var available, out _);
                    return worker - available;
                }
            }

            /// <summary>
            /// Units for the probe
            /// </summary>
            public override String Unit => null;
        }

        /// <summary>
        /// Thread pool performance probe
        /// </summary>
        public NetThreadPoolPerformanceProbe()
        {
            this.m_performanceCounters = new IDiagnosticsProbe[]
            {
                new NetPooledWorkersProbe(),
                new NetIoPooledWorkersProbe()
            };
        }

        /// <summary>
        /// Get the UUID of the thread pool
        /// </summary>
        public Guid Uuid => PerformanceConstants.NetThreadPoolPerformanceCounter;

        /// <summary>
        /// Gets the value of the
        /// </summary>
        public IEnumerable<IDiagnosticsProbe> Value => this.m_performanceCounters;

        /// <summary>
        /// Gets thename of hte composite
        /// </summary>
        public string Name => ".NET Thread Pool";

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "Shows the current characteristics of the .NET thread pool";

        /// <summary>
        /// Gets the type of the performance counter
        /// </summary>
        public Type Type => typeof(Array);

        /// <summary>
        /// Gets the value
        /// </summary>
        object IDiagnosticsProbe.Value => this.Value;

        /// <summary>
        /// Units for the probe
        /// </summary>
        public String Unit => null;
    }
}
