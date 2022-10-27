using SanteDB.Core.Model.Audit;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Represents a service which starts up and binds itself to key system events
    /// </summary>
    public interface IAuditDaemon : IDaemonService
    {
    }
}
