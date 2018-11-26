using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a required configuration attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConfigurationRequiredAttribute : Attribute
    {
    }
}
