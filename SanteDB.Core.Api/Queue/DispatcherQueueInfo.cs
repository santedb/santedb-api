﻿/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-11-19
 */
using Newtonsoft.Json;
using SanteDB.Core.Model;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Queue
{
    /// <summary>
    /// Queue entry
    /// </summary>
    [XmlRoot(nameof(DispatcherQueueInfo), Namespace = "http://santedb.org/queue")]
    [XmlType(nameof(DispatcherQueueInfo), Namespace = "http://santedb.org/queue")]
    public class DispatcherQueueInfo
    {
        /// <summary>
        /// Serialization ctor
        /// </summary>
        public DispatcherQueueInfo()
        {
        }

        /// <summary>
        /// ID of the queue
        /// </summary>
        [XmlElement("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// Gets or sets the payload type
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Queue size
        /// </summary>
        [XmlElement("size"), JsonProperty("size")]
        public int QueueSize { get; set; }

        /// <summary>
        /// Get the creation time
        /// </summary>
        [XmlElement("creationTime"), JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }
    }
}