/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Notifications.Email
{

    public class EmailMessage
    {
        public IEnumerable<string> ToAddresses { get; set; }
        public IEnumerable<string> CcAddresses { get; set; }
        public IEnumerable<string> BccAddresses { get; set; }
        public string FromAddress { get; set; }
        public string Subject { get; set; }
        public object Body { get; set; }
        public bool HighPriority { get; set; }

        public IEnumerable<(string name, string contentType, object content)> Attachments { get; set; }


    }
}
