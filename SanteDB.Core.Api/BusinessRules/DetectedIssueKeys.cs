using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// Detected issue type keys
    /// </summary>
    public static class DetectedIssueKeys
    {

        /// <summary>
        /// Password failed validation
        /// </summary>
        public static readonly Guid SecurityIssue = Guid.Parse("1a4ff454-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Password failed validation
        /// </summary>
        public static readonly Guid FormalConstraintIssue = Guid.Parse("1a4ff6f2-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Codification issue
        /// </summary>
        public static readonly Guid CodificationIssue = Guid.Parse("1a4ff850-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Some other issue
        /// </summary>
        public static readonly Guid OtherIssue = Guid.Parse("1a4ff986-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Already performed
        /// </summary>
        public static readonly Guid AlreadyDoneIssue = Guid.Parse("1a4ffab2-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Invalid data 
        /// </summary>
        public static readonly Guid InvalidDataIssue = Guid.Parse("1a4ffcec-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Business rule violation
        /// </summary>
        public static readonly Guid BusinessRuleViolationIssue = Guid.Parse("1a4ffe40-f54f-11e8-8eb2-f2801f1b9fd1");

        /// <summary>
        /// Business rule violation
        /// </summary>
        public static readonly Guid SafetyConcernIssue = Guid.Parse("1a4fff6c-f54f-11e8-8eb2-f2801f1b9fd1");

    }
}
