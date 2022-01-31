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
 * Date: 2021-8-27
 */
using SanteDB.Core.Auditing;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service that dispatches audits to a central repository
    /// </summary>
    /// <remarks>
    /// <para>The auditing of access to clinical data is of the utmost importance. SanteDB generates 
    /// and stores audits locally using an <see cref="IRepositoryService"/> for <see cref="AuditData"/>. However, 
    /// many implementations will have centralized audit repositories for collecting audits from various health
    /// systems in a central place. Such collection is useful to establishing overall patterns of access
    /// across systems in an HIE (for example)</para>
    /// <para>The audit dispatching service is responsible for sending <see cref="AuditData"/> instances to remote
    /// audit repositories. The service's responsibilities are:</para>
    /// <list type="number">
    ///     <item>Ensure that the <see cref="AuditData"/> instance is complete and contains relevant information for this node</item>
    ///     <item>Transform the <see cref="AuditData"/> class into the appropriate format (IETF RFC3881, FHIR, etc.)</item>
    ///     <item>Ensure the delivery of the audit to the central repository</item>
    /// </list>
    /// </remarks>
    [System.ComponentModel.Description("Audit Dispatch Service")]
    public interface IAuditDispatchService : IServiceImplementation
    {
        /// <summary>
        /// Sends the audit to the central authority
        /// </summary>
        void SendAudit(AuditData audit);
    }
}
