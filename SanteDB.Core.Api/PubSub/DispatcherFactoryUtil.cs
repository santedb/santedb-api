/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Dispatcher factory utility
    /// </summary>
    public static class DispatcherFactoryUtil
    {
        // Cached factories
        private static IDictionary<String, IPubSubDispatcherFactory> m_factories;

        // Cached filter criteria
        private static ConcurrentDictionary<Guid, Func<Object, bool>> m_filterCriteria = new ConcurrentDictionary<Guid, Func<Object, bool>>();

        static DispatcherFactoryUtil()
        {
            var serviceManager = ApplicationServiceContext.Current.GetService<IServiceManager>();

            m_factories = serviceManager.GetAllTypes()
                    .Where(t => typeof(IPubSubDispatcherFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsAbstract)
                    .Select(t => serviceManager.CreateInjected(t))
                    .OfType<IPubSubDispatcherFactory>()
                    .ToDictionaryIgnoringDuplicates(k => k.Id, f => f);
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

        /// <summary>
        /// Filters subscriptions from a list of subscriptions where the definition is "interested" in the event and data provided
        /// </summary>
        public static IEnumerable<PubSubSubscriptionDefinition> FilterSubscriptionMatch(this IEnumerable<PubSubSubscriptionDefinition> me, PubSubEventType eventType, Object data)
        {
            if (data is ParameterCollection pc)
            {
                data = pc.Parameters.First().Value;
            }

            var resourceName = data.GetType().GetSerializationName();
            return me.Where(o => o.ResourceTypeName == resourceName && o.IsActive && (o.NotBefore == null || o.NotBefore < DateTimeOffset.Now) && (o.NotAfter == null || o.NotAfter > DateTimeOffset.Now))
                    .Where(o => o.Event.HasFlag(eventType))
                    .Where(s =>
                    {
                        // Attempt to compile the filter criteria into an executable function
                        if (!m_filterCriteria.TryGetValue(s.Key.Value, out Func<Object, bool> fn))
                        {
                            Expression dynFn = null;
                            var parameter = Expression.Parameter(data.GetType());

                            foreach (var itm in s.Filter)
                            {
                                var fFn = QueryExpressionParser.BuildLinqExpression(data.GetType(), itm.ParseQueryString(), "p", forceLoad: true, lazyExpandVariables: true);
                                if (dynFn is LambdaExpression le)
                                {
                                    dynFn = Expression.Lambda(
                                        Expression.And(
                                            Expression.Invoke(le, parameter),
                                            Expression.Invoke(fFn, parameter)
                                           ), parameter);
                                }
                                else
                                {
                                    dynFn = fFn;
                                }
                            }

                            if (dynFn == null)
                            {
                                dynFn = Expression.Lambda(Expression.Constant(true), parameter);
                            }
                            parameter = Expression.Parameter(typeof(object));
                            fn = Expression.Lambda(Expression.Invoke(dynFn, Expression.Convert(parameter, data.GetType())), parameter).Compile() as Func<Object, bool>;
                            m_filterCriteria.TryAdd(s.Key.Value, fn);
                        }
                        return fn(data);
                    });
        }
    }
}