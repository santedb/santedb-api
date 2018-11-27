﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Service instantiation type
    /// </summary>
    public enum ServiceInstantiationType
    {
        /// <summary>
        /// The service class is constructed once and one instance is shared in the entire application domain
        /// </summary>
        Singleton,
        /// <summary>
        /// The service class is instantiated for each call of GetService()
        /// </summary>
        Instance
    }

    /// <summary>
    /// Identifies the manner in which a service is executed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceProviderAttribute : Attribute
    {

        /// <summary>
        /// Identifies a service provider
        /// </summary>
        public ServiceProviderAttribute(String name, ServiceInstantiationType type = ServiceInstantiationType.Singleton)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public String Name { get; set; }
        
        /// <summary>
        /// Service type
        /// </summary>
        public ServiceInstantiationType Type { get; set; }
    }
}
