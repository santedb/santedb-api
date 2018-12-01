using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// Detected issue priority
    /// </summary>
    public enum DetectedIssuePriorityType : int
    {
        /// <summary>
        /// The issue is an error, processing cannot continue
        /// </summary>
		Error = 1,
        /// <summary>
        /// The issue is for information only
        /// </summary>
		Informational = 2,
        /// <summary>
        /// The issue is just a warning, processing will continue
        /// </summary>
		Warning = 4
    }

    /// <summary>
    /// Represents a detected issue
    /// </summary>
    [JsonObject(nameof(DetectedIssue))]
    [XmlType(nameof(DetectedIssue), Namespace = "http://santedb.org/issue")]
    public class DetectedIssue
    {


        public DetectedIssue()
        {

        }

        /// <summary>
        /// Creates a new detected issue
        /// </summary>
        public DetectedIssue(DetectedIssuePriorityType priority, String text, Guid type)
        {
            this.Priority = priority;
            this.Text = text;
            this.TypeKey = type;
        }

        /// <summary>
        /// Represents a detected issue priority
        /// </summary>
        [XmlAttribute("priority"), JsonProperty("priority")]
        public DetectedIssuePriorityType Priority { get; set; }

        /// <summary>
        /// Text related to the issue
        /// </summary>
        [XmlText, JsonProperty("text")]
        public String Text { get; set; }

        /// <summary>
        /// The type of issue (a concept)
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public Guid TypeKey { get; set; }

    }
}
