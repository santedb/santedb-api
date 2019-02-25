using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Attributes
{

    /// <summary>
    /// Identifies the type/environment
    /// </summary>
    [Flags]
    public enum PluginEnvironment
    {
        /// <summary>
        /// The plugin works on the server only
        /// </summary>
        Server = 0x1,
        /// <summary>
        /// The plugin works on the client environment only
        /// </summary>
        Client = 0x2,
        /// <summary>
        /// The plugin works either on server or client
        /// </summary>
        ServerOrMobile = Server | Client,
    }

    /// <summary>
    /// Represents a plugin attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets the type of plugin
        /// </summary>
        public PluginEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the group
        /// </summary>
        public String Group { get; set; }

        /// <summary>
        /// Gets or sets the enabling of the plugin by default
        /// </summary>
        public bool EnableByDefault { get; set; }


    }
}
