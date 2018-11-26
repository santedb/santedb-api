using SanteDB.Core.Auditing;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service that dispatches audits to a central repository
    /// </summary>
    public interface IAuditDispatchService 
    {

        void SendAudit(AuditData audit);
    }
}
