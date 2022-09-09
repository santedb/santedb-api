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
using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Subscription;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Contract which defines a dCDR subscription executor
    /// </summary>
    /// <remarks>
    /// <para>The subscription executor is responsible for the translation of a dCDR <see cref="SubscriptionDefinition"/>
    /// from the <see cref="ISubscriptionRepository"/> to the appropriate database technology. The subscription executor
    /// gathers any new records requested by the dCDR and prepares them for download by the dCDR.</para>
    /// </remarks>
    [System.ComponentModel.Description("dCDR Server Named Query Provider")]
    public interface ISubscriptionExecutor : IServiceImplementation
    {

        /// <summary>
        /// Occurs after a subscription has been executed, and allows subscribers to modify the data being
        /// sent back to the dCDR
        /// </summary>
        event EventHandler<QueryResultEventArgs<IdentifiedData>> Executed;

        /// <summary>
        /// Occurs prior to a subscription being executed, and allows subscribers to modify the query being
        /// executed.
        /// </summary>
        event EventHandler<QueryRequestEventArgs<IdentifiedData>> Executing;

        /// <summary>
        /// Executes the identified subscription agianst the persistence layer.
        /// </summary>
        /// <param name="queryDefinitionKey">The key of the query definition to run</param>
        /// <param name="parameters">The parameters from the query</param>
        /// <param name="offset">The start record</param>
        /// <param name="count">The number of records</param>
        /// <param name="totalResults">The total results in the subscription</param>
        /// <param name="queryId">The query identifier</param>
        /// <returns>The results from the execution</returns>
        IEnumerable<Object> Execute(Guid queryDefinitionKey, NameValueCollection parameters, int offset, int? count, out int totalResults, Guid queryId);

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
