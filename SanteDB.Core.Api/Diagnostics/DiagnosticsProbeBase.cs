using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Represents a performance counter base class
    /// </summary>
    public abstract class DiagnosticsProbeBase<TMeasure> : IDiagnosticsProbe<TMeasure>
        where TMeasure : struct
    {

        /// <summary>
        /// Base performance counter
        /// </summary>
        public DiagnosticsProbeBase(String name, String description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Get the value of the measure
        /// </summary>
        public abstract TMeasure Value { get; }

        /// <summary>
        /// Gets the identifier for the counter
        /// </summary>
        public abstract Guid Uuid { get; }

        /// <summary>
        /// Get the name of the measure
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the measure
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the type of the measure
        /// </summary>
        public Type Type => typeof(TMeasure);

        /// <summary>
        /// Gets the value
        /// </summary>
        object IDiagnosticsProbe.Value => this.Value;
    }
}
