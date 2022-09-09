/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2022-5-30
 */
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