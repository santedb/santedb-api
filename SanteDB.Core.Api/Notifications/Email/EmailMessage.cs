using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Notifications.Email
{

    public class EmailMessage
    {
        public IEnumerable<string> ToAddresses { get; set; }
        public IEnumerable<string> CcAddresses { get; set; }
        public IEnumerable<string> BccAddresses { get; set; }
        public string FromAddress { get; set; }
        public string Subject { get; set; }
        public object Body { get; set; }
        public bool HighPriority { get; set; }

        public IEnumerable<(string name, string contentType, object content)> Attachments { get; set; }


    }
}
