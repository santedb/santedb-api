/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Contract for services which enforce privacy directives
    /// </summary>
    /// <remarks>
    /// <para>Implementers of this service contract are expected to provide support for the 
    /// <see href="https://help.santesuite.org/santedb/privacy-architecture">SanteDB Privacy Enforcement</see> architecture. The responsibilities for 
    /// implementers are:</para>
    /// <list type="bullet">
    ///     <item>Enforce the data privacy directives attached to <see cref="Entity"/> or <see cref="Act"/> instances prior to disclosure of the record (for example: redact, mask, or hide)</item>
    ///     <item>Ensure that data privacy directives are adhered to prior to updating data in the CDR</item>
    ///     <item>Ensure that fields which are sensitive or forbidden are not being used in queries</item>
    /// </list>
    /// <para>This service is used by the <see cref="IRepositoryService"/> layer. <strong>ValidateWrite</strong> is used prior to executing a
    /// write operation should ensure that the data being provided/written does not violate local privacy laws (i.e. if Race is forbidden, and the 
    /// request contains Race the request should be aborted or scrubbed)</para>
    /// <para>Additionally, the <strong>ValidateQuery</strong> method is invoked prior to querying to ensure that the query parameters don't
    /// violate local privacy laws (i.e. don't permit query on MaritalStatus) and that patient privacy policies would not be violated by the query.
    /// For example, if the jurisdiction has a policy which protects or hides <c>HIV_PROGRAM</c> identifiers, and a principal which lacks that policy
    /// attempts a query such as <c>identifier[HIV_PROGRAM].value=!null</c>, then patient privacy could be compromised just by the nature of a 
    /// a result being returned (even it if the HIV_PROGRAM identifier is scrubbed). The <strong>ValidateQuery</strong> method should protect in these
    /// cases (note: the default implementation does not protect against this, however the capability is present for third party implementers of this service
    /// to produce such behavior)</para>
    /// </remarks>
    [System.ComponentModel.Description("Data Privacy Enforcement Provider")]
    public interface IPrivacyEnforcementService : IServiceImplementation
    {
        /// <summary>
        /// Applies the privacy policies attached to the provided data such that a disclosure to the provided principal would
        /// not compromise patient privacy.
        /// </summary>
        /// <remarks>Implementers are expected to consider the policies currently applied to <paramref name="data"/> and 
        /// take appropriate actions for those policies given the record will be disclosed to <paramref name="principal"/></remarks>
        /// <param name="data">The data which is about to be disclosed</param>
        /// <param name="principal">The security principal to which the <paramref name="data"/> is being disclosed</param>
        /// <returns>The data which will actually be disclosed</returns>
        TData Apply<TData>(TData data, IPrincipal principal) where TData : IdentifiedData;

        /// <summary>
        /// Apply the policies on any result in <paramref name="data"/> such that patient privacy of any returned
        /// record would not compromise patient privacy.
        /// </summary>
        /// <param name="data">The results which are about to be disclosed to <paramref name="principal"/></param>
        /// <param name="principal">The security principal to which the <paramref name="data"/> is being disclosed</param>
        /// <returns>The data which will actually be disclosed</returns>
        IQueryResultSet<TData> Apply<TData>(IQueryResultSet<TData> data, IPrincipal principal) where TData : IdentifiedData;

        /// <summary>
        /// Determine if the record provided contains data that the user <paramref name="principal"/>
        /// shouldn't be sending.
        /// </summary>
        /// <remarks><para>Implementers are expected to provide a write validation routine which ensures that the <paramref name="principal"/>
        /// is not sending data in <paramref name="data"/> which:</para>
        /// <list type="bullet">
        ///     <item>Violates local jurisdictional laws (i.e. forbidden or restricted fields)</item>
        ///     <item>Has masked, incomplete, or unsafe data (i.e. the client downloaded a masked record and is resubmitting it)</item>
        /// </list>
        /// </remarks>
        bool ValidateWrite<TData>(TData data, IPrincipal principal) where TData : IdentifiedData;

        /// <summary>
        /// Validate that a query can be performed by user <paramref name="principal"/> and does not contain forbidden or compromising fields
        /// </summary>
        /// <remarks><para>
        /// Some types of queries may violate or may compromise patient privacy. This method is used by the <see cref="IRepositoryService"/> prior
        /// to a query being performed to ensure that:
        /// </para>
        /// <list type="number">
        ///     <item>The query is not using fields which have been configured as forbidden by the jurisdiction</item>
        ///     <item>The query does not contain explicit queries for data which, when masked, would indicate the condition. For example,
        ///     if records contain a policy "HIDE HIV programme identifiers", the disclosure of the identifier would be protected via
        ///     the <see cref="Apply{TData}(IEnumerable{TData}, IPrincipal)"/> method, however, if a principal explicitly queried
        ///     for <c>identifier[HIV_PROGRAM].value=!null</c> they would still be disclosed patients which have an HIV program identifier. This method
        ///     should search the <paramref name="query"/> provided and ensure that <paramref name="principal"/> is not violating such conditions.</item>
        /// </list>
        /// </remarks>
        /// <typeparam name="TModel">The type of object being filtered</typeparam>
        /// <param name="query">The query being executed</param>
        /// <param name="principal">The principal who is executing the query</param>
        /// <returns>True if the query can be executed in s asafe manner</returns>
        bool ValidateQuery<TModel>(Expression<Func<TModel, bool>> query, IPrincipal principal) where TModel : IdentifiedData;


    }
}