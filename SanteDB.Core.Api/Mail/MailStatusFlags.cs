using System.Xml.Serialization;

namespace SanteDB.Core.Mail
{
    /// <summary>
    /// Flags for a mailbox message in a folder
    /// </summary>
    [XmlType(nameof(MailMessageFlags), Namespace = "http://santedb.org/model")]
    public enum MailStatusFlags
    {
        /// <summary>
        /// Identifies a mail message as unread
        /// </summary>
        [XmlEnum("u")]
        Unread = 0x0,
        /// <summary>
        /// Identifies a mail message a read
        /// </summary>
        [XmlEnum("r")]
        Read = 0x1,
        /// <summary>
        /// Identifies a mail message as flagged
        /// </summary>
        [XmlEnum("f")]
        Flagged = 0x2,
        /// <summary>
        /// Identifies the mail message has been marked as complete
        /// </summary>
        [XmlEnum("c")]
        Complete = 0x4

    }
}