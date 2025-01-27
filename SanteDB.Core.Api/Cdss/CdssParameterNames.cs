using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// CDSS parameter names
    /// </summary>
    public static class CdssParameterNames
    {

        /// <summary>
        /// The encounter for which the care plan is being generated
        /// </summary>
        public const string ENCOUNTER_SCOPE = "encounter";
        /// <summary>
        /// The care pathway under which the care plan is being generated
        /// </summary>
        public const string PATHWAY_SCOPE = "pathway";
        /// <summary>
        /// The protocols which should be run
        /// </summary>
        public const string PROTOCOL_IDS = "runProtocols";
        /// <summary>
        /// True if the output is going to be persisted to the database
        /// </summary>
        public const string PERSISTENT_OUTPUT = "_persistent";
        /// <summary>
        /// True if supplements should be excluded
        /// </summary>
        public const string EXCLUDE_SUPPLEMENT = "_excludeSupplements";
        /// <summary>
        /// True if administrations of products should be excluded
        /// </summary>
        public const string EXCLUDE_ADMINISTRATIONS = "_excludeAdministrations";
        /// <summary>
        /// True if the observations should be excluded
        /// </summary>
        public const string EXCLUDE_OBSERVATIONS = "_excludeObservations";
        /// <summary>
        /// Exclude all proposals
        /// </summary>
        public const string EXCLUDE_PROPOSALS = "_excludePropose";
        
        /// <summary>
        /// Exclude all issues
        /// </summary>
        public const string EXCLUDE_ISSUES = "_excludeIssue";
        /// <summary>
        /// Any historical data should also be included and tagged as back-entry
        /// </summary>
        public const string INCLUDE_BACKENTRY = "_includeBackentry";
        /// <summary>
        /// True if the CDSS is being run in non-interactive mode
        /// </summary>
        public const string NON_INTERACTIVE = "isBackground";
        /// <summary>
        /// True if the CDSS is being run in test mode (emit debug/trace data)
        /// </summary>
        public const string TESTING = "isTesting";
        /// <summary>
        /// True if the CDSS is being run in debug mode
        /// </summary>
        public const string DEBUG_MODE = "debug";
        /// <summary>
        /// The time for which the events must fall within to be emitted
        /// </summary>
        public const string PERIOD_OF_EVENTS = "period";
        /// <summary>
        /// Only return the first applicable action from each protocol
        /// </summary>
        public const string FIRST_APPLICAPLE = "firstOnly";

        /// <summary>
        /// The execution mode
        /// </summary>
        public const string EXECUTION_MODE = "_mode";
    }
}
