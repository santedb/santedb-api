/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
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