/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using System.Collections.Generic;

namespace SanteDB.Core.Notifications.Email
{

    /// <summary>
    /// Represents a SanteDB internal mail message between clients and users
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Addresses which are addressed for this message
        /// </summary>
        public IEnumerable<string> ToAddresses { get; set; }
        /// <summary>
        /// The addresses which are to be CC'd
        /// </summary>
        public IEnumerable<string> CcAddresses { get; set; }
        /// <summary>
        /// The blind carbon copy address
        /// </summary>
        public IEnumerable<string> BccAddresses { get; set; }
        /// <summary>
        /// The address which is sending this message
        /// </summary>
        public string FromAddress { get; set; }
        /// <summary>
        /// Gets or sets the subject of the mail message
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Gets or sets the body of the message
        /// </summary>
        public object Body { get; set; }
        /// <summary>
        /// Gets or sets whether the message is high priority
        /// </summary>
        public bool HighPriority { get; set; }

        /// <summary>
        /// Get the attachmets which are attached to the message
        /// </summary>
        public IEnumerable<(string name, string contentType, object content)> Attachments { get; set; }


    }
}
