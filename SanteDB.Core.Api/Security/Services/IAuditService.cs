/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Services;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Auditing Service for SanteDB. <see cref="IAuditService"/> replaces the obsolete <see cref="AuditUtil"/> static implementation.
    /// </summary>
    public interface IAuditService : IServiceImplementation
    {
        /// <summary>
        /// Creates a new <see cref="IAuditBuilder"/> instance tied to this service for dispatch.
        /// </summary>
        /// <returns>A builder that can be used to construct and dispatch the audit.</returns>
        IAuditBuilder Audit();


        /// <summary>
        /// Sends an audit event to be processed. 
        /// </summary>
        /// <param name="audit">The audit to send.</param>
        /// <remarks>This is an asynchronous operation. The call will return as soon as the audit event is on the dispatcher queue. It will not wait for the audit event to be processed.</remarks>
        void SendAudit(AuditEventData audit);

        /// <summary>
        /// Directly dispatches the audit to a local repository or an audit dispatcher.
        /// </summary>
        /// <param name="audit">The audit to dispatch.</param>
        /// <remarks>This is a blocking call if the dispatch configuration will send to a remote service. To Asynchronously send an audit event, use <see cref="SendAudit(AuditEventData)"/> instead.</remarks>
        void DispatchAudit(AuditEventData audit);
    }
}
