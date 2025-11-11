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
using System.Diagnostics;
using System.IO;
#if DEBUG

namespace SanteDB.Core.Diagnostics.Performance
{

    /// <summary>
    /// Gets or sets the performance tracer if a query takes more than 1 second to execute
    /// </summary>
    public static class PerformanceTracer
    {
        private static object syncLock = new object();
        private static long sequence = 0;

        /// <summary>
        /// Writes a perform tracer to 
        /// </summary>
        /// <param name="milliseconds"></param>
        public static void WritePerformanceTrace(long milliseconds)
        {
            if (milliseconds > 1000 && ApplicationServiceContext.Current.HostType == SanteDBHostType.Server)
            {
                var stack = new StackTrace(false).GetFrame(1).GetMethod();
                lock (syncLock)
                {
                    using (var tw = File.AppendText("perfmon.csv"))
                    {
                        tw.WriteLine("{0},\"{1}\",\"{2}\",{3}", sequence++, stack.DeclaringType, stack, milliseconds);
                    };
                }
            }
        }

    }
}
#endif
