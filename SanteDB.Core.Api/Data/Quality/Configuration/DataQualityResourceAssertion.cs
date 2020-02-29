using SanteDB.Core.BusinessRules;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Represents a single assertion on a resource
    /// </summary>
    [XmlType(nameof(DataQualityResourceAssertion), Namespace = "http://santedb.org/configuration")]
    public class DataQualityResourceAssertion
    {

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name 
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority 
        /// </summary>
        [XmlAttribute("priority")]
        public DetectedIssuePriorityType Priority { get; set; }

        /// <summary>
        /// The evaluation
        /// </summary>
        [XmlAttribute("evaluation")]
        public AssertionEvaluationType Evaluation { get; set; }

        /// <summary>
        /// Gets or sets the expressions which are checked
        /// </summary>
        [XmlElement("expression")]
        public List<string> Expressions { get; set; }
    }

    /// <summary>
    /// Assertion evaluation type
    /// </summary>
    [XmlType(nameof(AssertionEvaluationType), Namespace = "http://santedb.org/configuration")]
    public enum AssertionEvaluationType
    {
        /// <summary>
        /// All of the expressions must evaluate to true
        /// </summary>
        [XmlEnum("all")]
        All, 
        /// <summary>
        /// Any of the expressions must evaluate to true
        /// </summary>
        [XmlEnum("any")]
        Any,
        /// <summary>
        /// None of the expressions should evaluate to true
        /// </summary>
        [XmlEnum("none")]
        None
    }
}