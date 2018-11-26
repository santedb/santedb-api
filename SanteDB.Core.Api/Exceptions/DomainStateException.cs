using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// An indicator exception to illustrate that the domain is not yet ready to be accessed
    /// </summary>
    public class DomainStateException : Exception
    {
    }
}
