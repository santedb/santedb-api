using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// A marker class that indicates the configuration section is encrypted in the configuration file
    /// </summary>
    public interface IEncryptedConfigurationSection : IConfigurationSection
    {
    }
}
