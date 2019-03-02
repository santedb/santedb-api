using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Features
{

    /// <summary>
    /// Represents a generic configuration option object which will cause any software 
    /// to instead use a type descritpor
    /// </summary>
    public class GenericFeatureConfiguration
    {

        /// <summary>
        /// Generic feature configuration
        /// </summary>
        public GenericFeatureConfiguration()
        {
            this.Options = new Dictionary<string, Func<Object>>();
            this.Values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the configuration options for this generic feature
        /// </summary>
        public Dictionary<String, Func<Object>> Options { get;  }

        /// <summary>
        /// Gets the current set configuration values
        /// </summary>
        public Dictionary<String, Object> Values { get; }
    }
}
