using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Attributes
{
    /// <summary>
    /// Allows a plugin to delcare a dependency
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class PluginDependencyAttribute : Attribute
    {

        /// <summary>
        /// Dependency AQN
        /// </summary>
        public PluginDependencyAttribute(string dependencyAqn)
        {
            this.Dependency = dependencyAqn;
        }

        /// <summary>
        /// Gets or sets the assembly dependency 
        /// </summary>
        public String Dependency { get; set; }

    }
}
