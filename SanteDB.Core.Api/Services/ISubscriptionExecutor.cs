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
using SanteDB.Core.Event;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Subscription;
using System;
using System.Collections.Specialized;
using System.Security.Principal;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Event args for after subscription has been executed
    /// </summary>
    public class SubscriptionExecutedEventArgs : SecureAccessEventArgs
    {
        /// <summary>
        /// Creates a new instance of the subscription executed event args
        /// </summary>
        /// <param name="subscriptionDefinition">The subscription definition fired</param>
        /// <param name="parameters">The parameters used to execute</param>
        /// <param name="results">The results returned from the query</param>
        /// <param name="principal">The principal which executed the query</param>
        public SubscriptionExecutedEventArgs(SubscriptionDefinition subscriptionDefinition, NameValueCollection parameters, IQueryResultSet results, IPrincipal principal) : base(principal)
        {
            this.SubscriptionDefinition = subscriptionDefinition;
            this.Parameters = parameters;
            this.Results = results;
        }

        /// <summary>
        /// Gets the subscription definition to be executed
        /// </summary>
        public SubscriptionDefinition SubscriptionDefinition { get; }

        /// <summary>
        /// Gets the parameters passed to the subscription filter
        /// </summary>
        public NameValueCollection Parameters { get; }

        /// <summary>
        /// Gets the results which have been executed
        /// </summary>
        public virtual IQueryResultSet Results { get; set; }


    }
    /// <summary>
    /// Event args for the pre-fire of a subscription being executed
    /// </summary>
    public class SubscriptionExecutingEventArgs : SubscriptionExecutedEventArgs
    {

        /// <summary>
        /// Creates a new subscription executing event args structure
        /// </summary>
        /// <param name="subscriptionDefinition">The definition of the subscription being executed</param>
        /// <param name="parameters">The parameters to be passed to the query</param>
        /// <param name="principal">The principal executing the query</param>
        public SubscriptionExecutingEventArgs(SubscriptionDefinition subscriptionDefinition, NameValueCollection parameters, IPrincipal principal) : base(subscriptionDefinition, parameters, null, principal)
        {
        }

        /// <summary>
        /// Set to true if the handler wishes to cancel the original caller's execution
        /// </summary>
        public bool Cancel { get; set; }


    }

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
        event EventHandler<SubscriptionExecutedEventArgs> Executed;

        /// <summary>
        /// Occurs prior to a subscription being executed, and allows subscribers to modify the query being
        /// executed.
        /// </summary>
        event EventHandler<SubscriptionExecutingEventArgs> Executing;

        /// <summary>
        /// Executes the identified subscription agianst the persistence layer.
        /// </summary>
        /// <param name="queryDefinitionKey">The key of the query definition to run</param>
        /// <param name="parameters">The parameters from the query</param>
        /// <returns>The results from the execution</returns>
        IQueryResultSet Execute(Guid queryDefinitionKey, NameValueCollection parameters);

        /// <summary>
        /// Executes the provided subscription definition
        /// </summary>
        /// <param name="subscription">The loaded subscription definition to be used</param>
        /// <param name="parameters">The parameters to query</param>
        /// <returns>The results matching the filter parameters</returns>
        IQueryResultSet Execute(SubscriptionDefinition subscription, NameValueCollection parameters);
    }
}
