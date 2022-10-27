using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Constants for audit related classes
    /// </summary>
    public static class AuditConstants
    {
        /// <summary>
        /// The name of the dispatcher queue for audits
        /// </summary>
        public const string QueueName = "sys.audit";
        /// <summary>
        /// The name of the dead-letter queue for audits
        /// </summary>
        public const string DeadletterQueueName = QueueName + ".dead";
    }
}
