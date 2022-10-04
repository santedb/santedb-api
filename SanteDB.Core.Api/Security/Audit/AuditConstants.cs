using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Constants for audit related classes
    /// </summary>
    public static class AuditConstants
    {
        public const string QueueName = "sys.audit";
        public const string DeadletterQueueName = QueueName + ".dead";
    }
}
