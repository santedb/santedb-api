using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// This attribute tells the dependency injection manager that the current 
    /// implementation should be treated as the default when GetService is called
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PreferredServiceAttribute : Attribute
    {

        /// <summary>
        /// Creates a new preferred service attribute
        /// </summary>
        /// <param name="serviceType">The service type this implementation is preferred implementation for</param>
        public PreferredServiceAttribute(Type serviceType)
        {
            this.ServiceType = serviceType;
        }

        /// <summary>
        /// The type of service which this service is preferred for
        /// </summary>
        public Type ServiceType { get; set; }
    }
}
