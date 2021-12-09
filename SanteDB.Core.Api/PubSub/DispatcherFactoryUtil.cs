using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Dispatcher factory utility
    /// </summary>
    public static class DispatcherFactoryUtil
    {
        // Cached factories
        private static IDictionary<String, IPubSubDispatcherFactory> m_factories;

        static DispatcherFactoryUtil()
        {
            var serviceManager = ApplicationServiceContext.Current.GetService<IServiceManager>();

            m_factories = serviceManager.GetAllTypes()
                    .Where(t => typeof(IPubSubDispatcherFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsAbstract)
                    .Select(t => serviceManager.CreateInjected(t))
                    .OfType<IPubSubDispatcherFactory>()
                    .ToDictionary(k => k.Id, f => f);
        }

        /// <summary>
        /// Get all factories
        /// </summary>
        public static IDictionary<String, IPubSubDispatcherFactory> GetFactories()
        {
            return m_factories;
        }

        /// <summary>
        /// Finds an implementation of the IDisptacherFactory which works for the specified URI
        /// </summary>
        public static IPubSubDispatcherFactory FindDispatcherFactoryByUri(Uri targetUri)
        {
            return m_factories.Values.FirstOrDefault(o => o.Schemes.Contains(targetUri.Scheme));
        }

        /// <summary>
        /// Finds an implementation of the IDisptacherFactory which works for the specified ID
        /// </summary>
        public static IPubSubDispatcherFactory FindDispatcherFactoryById(string id)
        {
            if (m_factories.TryGetValue(id, out IPubSubDispatcherFactory retVal))
            {
                return retVal;
            }
            return null;
        }

        /// <summary>
        /// Get dispatcher factory by type
        /// </summary>
        public static IPubSubDispatcherFactory FindDispatcherFactoryByType(Type factoryType)
        {
            // TODO: Optimize this , basically this ensures that the factory type is not
            // initialized more than once.
            return m_factories.Values.FirstOrDefault(o => o.GetType() == factoryType);
        }
    }
}