using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a generic claim
    /// </summary>
    public class GenericClaim : IClaim
    {

        /// <summary>
        /// Creates a new generic claim
        /// </summary>
        public GenericClaim(String type, String value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Gets the type
        /// </summary>
        public string Type {get;}

        /// <summary>
        /// Gets the value of the claim
        /// </summary>
        public string Value { get; }
    }
}
