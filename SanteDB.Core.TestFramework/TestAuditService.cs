using SanteDB.Core.Model.Audit;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SanteDB.Core.TestFramework
{
    [ServiceProvider("Test Audit Service")]
    public class TestAuditService : IAuditService
    {
        public string ServiceName => "Test Audit Service";

        public IAuditBuilder Audit() => new TestAuditBuilder(this);

        public void DispatchAudit(AuditEventData audit)
        {
            Debug.WriteLine(audit);
        }

        public void SendAudit(AuditEventData audit)
        {
            Debug.WriteLine(audit);
        }

        protected class TestAuditBuilder : IAuditBuilder
        {
            readonly TestAuditService _Service;

            public TestAuditBuilder(TestAuditService service)
            {
                Audit = new AuditEventData();
                _Service = service;
            }

            public AuditEventData Audit { get; }

            public void Send()
            {
                _Service?.SendAudit(Audit);
            }
        }
    }
}
