using SanteDB.Core.Model.Audit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Builder interface for using fluent syntax for audit construction and dispatch.
    /// </summary>
    public interface IAuditBuilder
    {
        /// <summary>
        /// Gets the <see cref="AuditEventData"/> that is the focal of this fluent builder.
        /// </summary>
        AuditEventData Audit { get; }
        /// <summary>
        /// Sends the audit data to the dispatcher.
        /// </summary>
        void Send();
    }
}
