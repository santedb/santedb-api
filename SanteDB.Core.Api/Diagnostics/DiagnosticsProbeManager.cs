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
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// The performance monitor class
    /// </summary>
    public class DiagnosticsProbeManager : IDisposable
    {
        /// <summary>
        /// Counters
        /// </summary>
        private IList<IDiagnosticsProbe> m_probes;

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
            var serviceManager = ApplicationServiceContext.Current?.GetService<IServiceManager>();
            if (serviceManager == null)
            {
                this.m_probes = new List<IDiagnosticsProbe>();
            }
            else
            {
                this.m_probes = serviceManager.CreateInjectedOfAll<IDiagnosticsProbe>().ToList();
            }
        }

        /// <summary>
        /// Gets the current performance monitor
        /// </summary>
        public static DiagnosticsProbeManager Current
        {
            get
            {
                if (m_current == null)
                {
                    lock (m_lockObject)
                    {
                        if (m_current == null)
                        {
                            m_current = new DiagnosticsProbeManager();
                        }
                    }
                }

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
        public IEnumerable<IDiagnosticsProbe> Find(Func<IDiagnosticsProbe, bool> query)
        {
            var matches = this.m_probes.Where(query);
            return matches;
        }

        /// <summary>
        /// Add the specified probe
        /// </summary>
        public void Add(IDiagnosticsProbe probe)
        {
            lock (m_lockObject)
            {
                if (!this.m_probes.Any(p => p.Uuid == probe.Uuid))
                {
                    this.m_probes.Add(probe);
                }
            }
        }

        /// <summary>
        /// Dispose probes
        /// </summary>
        public void Dispose()
        {
            foreach(var probe in this.m_probes)
            {
                if(probe is IDisposable disp)
                {
                    disp.Dispose();
                }
            }
        }
    }
}