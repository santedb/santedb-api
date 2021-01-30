using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Configuration
{
    /// <summary>
    /// Data policy filter configuration
    /// </summary>
    [XmlType(nameof(DataPolicyFilterConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataPolicyFilterConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Gets or sets the default action
        /// </summary>
        [XmlAttribute("action"), JsonProperty("action")]
        public ResourceDataPolicyActionType DefaultAction { get; set; }

        /// <summary>
        /// Gets the list of resources
        /// </summary>
        [XmlArray("resources"), XmlArrayItem("add"), JsonProperty("resources")]
        public List<ResourceDataPolicyFilter> Resources { get; set; }

    }

    /// <summary>
    /// Resource filter policy configuration element
    /// </summary>
    [XmlType(nameof(ResourceDataPolicyFilter), Namespace = "http://santedb.org/configuration")]

    public class ResourceDataPolicyFilter : ResourceTypeReferenceConfiguration
    {

        /// <summary>
        /// Gets or sets the action
        /// </summary>
        [XmlAttribute("action")]
        public ResourceDataPolicyActionType Action { get; set; }
    }

    /// <summary>
    /// The resource data policy action
    /// </summary>
    [XmlType(nameof(ResourceDataPolicyActionType), Namespace = "http://santedb.org/configuration")]
    public enum ResourceDataPolicyActionType
    {
        /// <summary>
        /// None - Take no action
        /// </summary>
        [XmlEnum("none")]
        None,
        /// <summary>
        /// Only audit that the resource was disclosed and allow disclosure
        /// </summary>
        [XmlEnum("audit")]
        Audit,
        /// <summary>
        /// Disclose the record but mask the populated properties
        /// </summary>
        [XmlEnum("redact")]
        Redact,
        /// <summary>
        /// Disclose the record type and key, but clear all data
        /// </summary>
        [XmlEnum("nullify")]
        Nullify,
        /// <summary>
        /// Hide the record - return nothing
        /// </summary>
        [XmlEnum("hide")]
        Hide,
        /// <summary>
        /// Generate an error condition
        /// </summary>
        [XmlEnum("error")]
        Error
    }
}
