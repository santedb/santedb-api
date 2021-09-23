using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service factory which is capable of creating services
    /// </summary>
    public interface IServiceFactory
    {

        /// <summary>
        /// Try to create the specified service
        /// </summary>
        bool TryCreateService<TService>(out TService serviceInstance);

        /// <summary>
        /// Try to create specified service
        /// </summary>
        bool TryCreateService(Type serviceType, out object serviceInstance);
    }
}
