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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a generic interface for business rules services
    /// </summary>
    [System.ComponentModel.Description("Business Rules Engine")]
    public interface IBusinessRulesService
    {
        /// <summary>
        /// Gets the next BRE
        /// </summary>
        IBusinessRulesService Next { get; }

        /// <summary>
        /// Called after an insert occurs
        /// </summary>
        Object AfterInsert(Object data);

        /// <summary>
        /// Called after obsolete committed
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Object AfterObsolete(Object data);

        /// <summary>
        /// Called after query
        /// </summary>
        IQueryResultSet AfterQuery(IQueryResultSet results);

        /// <summary>
        /// Called after retrieve
        /// </summary>
        Object AfterRetrieve(Object result);

        /// <summary>
        /// Called after update committed
        /// </summary>
        Object AfterUpdate(Object data);

        /// <summary>
        /// Called before an insert occurs
        /// </summary>
        Object BeforeInsert(Object data);

        /// <summary>
        /// Called before obsolete
        /// </summary>
        Object BeforeObsolete(Object data);

        /// <summary>
        /// Called before an update occurs
        /// </summary>
        Object BeforeUpdate(Object data);

        /// <summary>
        /// Called to validate a specific object
        /// </summary>
        List<DetectedIssue> Validate(Object data);
    }

    /// <summary>
    /// Represents a service that executes business rules based on triggers which happen in the <see cref="IRepositoryService"/> layer
    /// </summary>
    /// <remarks>
    /// <para>When a business rules implementation is attached to the service context, or via the <c>AddBusinessRule</c> method,
    /// the SanteDB server call the appropiate Before/After functions on the implementation, before checking the <c>Next</c> property
    /// to follow the next business rule in the chain.</para>
    /// <para>The <see href="https://help.santesuite.org/developers/applets/business-rules">JavaScript Business Rules Engine</see> which loads
    /// data from installed applets is an example of an implementation of this service which translates events into Javascript callbacks. Implementers can use
    /// this service to:</para>
    /// <list type="bullet">
    ///     <item>Generate unique identifiers or other data and affix it to data</item>
    ///     <item>Intercept queries and write requests and perform re-writes</item>
    ///     <item>Log, catalog, or update external indexes of data</item>
    ///     <item>Cancel or interrupt the default flow of a persistence or query operation</item>
    /// </list>
    /// <para>Note: This can be done, instead with events on the persistence layer on the SanteDB datalayer, however there
    /// may come a time when a rule is fired without persistence occurring</para>
    /// </remarks>
    [Description("Business Rules Service")]
    public interface IBusinessRulesService<TModel> : IBusinessRulesService, IServiceImplementation where TModel : IdentifiedData
    {
        /// <summary>
        /// Gets or sets the rule to be run after this rule (for chained rules)
        /// </summary>
        new IBusinessRulesService<TModel> Next { get; set; }

        /// <summary>
        /// Called after an insert occurs.
        /// </summary>
        /// <remarks>Data modified or returned in this event is not persisted</remarks>
        /// <param name="data">The data that was inserted</param>
        /// <returns>The data which is to be returned to the caller</returns>
        TModel AfterInsert(TModel data);

        /// <summary>
        /// Called after obsolete has been committed
        /// </summary>
        /// <param name="data">The data which was obsoleted</param>
        /// <returns>The data which is to be returned to the caller</returns>
        TModel AfterDelete(TModel data);

        /// <summary>
        /// Called after a query has been executed
        /// </summary>
        /// <remarks>This method is useful if your business rule wishes to perform special (non-persisted) tagging, modification or masking
        /// of objects based on custom business logic</remarks>
        /// <param name="results">The results of the query</param>
        /// <returns>The modified query results</returns>
        IQueryResultSet<TModel> AfterQuery(IQueryResultSet<TModel> results);

        /// <summary>
        /// Called after retrieve of an object
        /// </summary>
        /// <param name="result">The result that was retrieved by the repository</param>
        /// <returns>The result which is to be passed to the caller</returns>
        TModel AfterRetrieve(TModel result);

        /// <summary>
        /// Called after update has been committed
        /// </summary>
        /// <param name="data">The data which was updated (the current version after update completed)</param>
        /// <returns>The data which is to be passed to the caller</returns>
        TModel AfterUpdate(TModel data);

        /// <summary>
        /// Called before an insert occurs
        /// </summary>
        /// <returns>The data which is to be passed to the persistence layer</returns>
        /// <param name="data">The data which is to be inserted</param>
        /// <remarks>The data returned form this method is what will be committed to the database regardless of the data passed in <paramref name="data"/>. 
        /// This method is useful for assigning new identifiers, tags, home facilities, or other information.</remarks>
        TModel BeforeInsert(TModel data);

        /// <summary>
        /// Called before obsolete occurs
        /// </summary>
        /// <param name="data">The data which is about to be obsoleted</param>
        /// <returns>The data object to be passed to the persistence layer for obsoletion</returns>
        /// <remarks>Implementers can use this method to change tags, states, extensions or notes on the object prior to the object being 
        /// committed to the data persistence layer.</remarks>
        TModel BeforeDelete(TModel data);

        /// <summary>
        /// Called before an update occurs
        /// </summary>
        /// <param name="data">The data which was to be updated</param>
        /// <returns>The actual data which should be committed</returns>
        TModel BeforeUpdate(TModel data);

        /// <summary>
        /// Called to validate the data provided
        /// </summary>
        /// <remarks>This method is used to raise <see cref="DetectedIssue"/> information statements based on 
        /// custom validation logic provided. For example, implementers can use this method to validate sufficient
        /// stock exists to perform an operation, to validate that an action is safe, etc.</remarks>
        /// <param name="data">The data which is to be validated</param>
        /// <returns>A list of <see cref="DetectedIssue"/> instances which represent the validation statements.</returns>
        List<DetectedIssue> Validate(TModel data);
    }
}