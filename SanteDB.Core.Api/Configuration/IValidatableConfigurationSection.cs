using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// A <see cref="IConfigurationSection"/> which supports validation
    /// </summary>
    public interface IValidatableConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Validate this instance of the configuration section
        /// </summary>
        IEnumerable<DetectedIssue> Validate();
    }
}
