using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a claim abstraction
    /// </summary>
    public interface IClaim
    {
        /// <summary>
        /// Gets the type of claim
        /// </summary>
        String Type { get; }

        /// <summary>
        /// Gets the value of the claim
        /// </summary>
        String Value { get; }

    }
}
