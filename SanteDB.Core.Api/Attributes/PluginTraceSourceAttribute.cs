using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Attributes
{
    /// <summary>
    /// Represents a trace source attribute which is used to control the diagnostics configuration for the plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class PluginTraceSourceAttribute : Attribute
    {

        /// <summary>
        /// Plugin trace source
        /// </summary>
        /// <param name="traceSource"></param>
        public PluginTraceSourceAttribute(String traceSource)
        {
            this.TraceSourceName = traceSource;
        }

        /// <summary>
        /// Gets or sets the trace source name
        /// </summary>
        public String TraceSourceName { get; set; }

    }
}
