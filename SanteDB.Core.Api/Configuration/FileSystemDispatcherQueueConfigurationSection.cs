using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a configuration section for file system queueing
    /// </summary>
    [XmlType(nameof(FileSystemDispatcherQueueConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class FileSystemDispatcherQueueConfigurationSection : IConfigurationSection
    {
        /// <summary>
        /// Gets or sets the path to the queue location
        /// </summary>
        [XmlAttribute("queueRoot")]
        [Description("Identifies where file system queues should be created")]
        [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public String QueuePath { get; set; }
    }
}