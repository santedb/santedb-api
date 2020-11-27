/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a subscription executor
    /// </summary>
    public interface ISubscriptionExecutor : IServiceImplementation
    {

        /// <summary>
        /// Occurs when queried.
        /// </summary>
        event EventHandler<QueryResultEventArgs<IdentifiedData>> Executed;

        /// <summary>
        /// Occurs when querying.
        /// </summary>
        event EventHandler<QueryRequestEventArgs<IdentifiedData>> Executing;

        /// <summary>
        /// Executes the specified subscription mechanism
        /// </summary>
        /// <param name="subscriptionKey">The key of the subscription to run</param>
        /// <param name="parameters">The parameters from the query</param>
        /// <param name="offset">The start record</param>
        /// <param name="count">The number of records</param>
        /// <param name="totalResults">The total results in the subscription</param>
        /// <param name="queryId">The query identifier</param>
        /// <returns>The results from the execution</returns>
        IEnumerable<Object> Execute(Guid subscriptionKey, NameValueCollection parameters, int offset, int? count, out int totalResults, Guid queryId);

        /// <summary>
        /// Executes the provided subscription definition
        /// </summary>
        /// <param name="subscription">The loaded subscription definition to be used</param>
        /// <param name="parameters">The parameters to query</param>
        /// <param name="offset">The offset of the first record</param>
        /// <param name="count">The number of results</param>
        /// <param name="totalResults">The total matching results</param>
        /// <param name="queryId">A stateful query identifier</param>
        /// <returns>The results matching the filter parameters</returns>
        IEnumerable<Object> Execute(SubscriptionDefinition subscription, NameValueCollection parameters, int offset, int? count, out int totalResults, Guid queryId);
    }
}
