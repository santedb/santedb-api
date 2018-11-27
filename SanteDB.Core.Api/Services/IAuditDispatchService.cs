using SanteDB.Core.Auditing;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service that dispatches audits to a central repository
    /// </summary>
    public interface IAuditDispatchService : IServiceImplementation
    {
        /// <summary>
        /// Sends the audit to the central authority
        /// </summary>
        void SendAudit(AuditData audit);
    }
}
