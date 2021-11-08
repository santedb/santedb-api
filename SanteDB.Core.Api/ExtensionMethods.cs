using SanteDB.Core.Interfaces;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core
{
    /// <summary>
    /// Extension methods for the core API functions
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create injected class
        /// </summary>
        public static object CreateInjected(this Type me)
        {
            return ApplicationServiceContext.Current.GetService<IServiceManager>().CreateInjected(me);
        }
    }
}