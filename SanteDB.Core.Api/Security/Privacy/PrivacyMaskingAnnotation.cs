using SanteDB.Core.Security.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Privacy
{
    /// <summary>
    /// Identifies the privacy masking reason as an annotation
    /// </summary>
    public struct PrivacyMaskingAnnotation
    {

        /// <summary>
        /// Privacy masking action CTOR
        /// </summary>
        public PrivacyMaskingAnnotation(PolicyDecision reason, ResourceDataPolicyActionType actionTaken)
        {
            this.MaskingReason = reason;
            this.ActionTaken = actionTaken;
        }

        /// <summary>
        /// Gets the reason (policy) the data was masked
        /// </summary>
        public PolicyDecision MaskingReason { get; }

        /// <summary>
        /// Identifies the action that was taken
        /// </summary>
        public ResourceDataPolicyActionType ActionTaken { get; }
    }
}
