using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
#if DEBUG

namespace SanteDB.Core.Diagnostics.Performance
{

    /// <summary>
    /// Gets or sets the performance tracer
    /// </summary>
    public static class PerformanceTracer
    {
        private static object syncLock = new object();
        private static long sequence = 0;

        public static void WritePerformanceTrace(long milliseconds)
        {
            if (milliseconds > 1000)
            {
                var stack = new StackTrace(false).GetFrame(1).GetMethod();
                lock (syncLock)
                {
                    using (var tw = File.AppendText("perfmon.csv"))
                    {
                        tw.WriteLine("{0},\"{1}\",\"{2}\",{3}", sequence++, stack.DeclaringType, stack, milliseconds);
                    };
                }
                // System.Diagnostics.Debugger.Break();
            }
        }

    }
}
#endif
