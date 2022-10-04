using SanteDB.Core.Model.Audit;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Auditing Service for SanteDB. <see cref="IAuditService"/> replaces the obsolete <see cref="Audit.AuditUtil"/> static implementation.
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
