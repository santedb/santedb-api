﻿/*
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
 * User: fyfej
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Identifies a single channel in the pub-sub manager
    /// </summary>
    [XmlType(nameof(PubSubChannelDefinition), Namespace = "http://santedb.org/pubsub")]
    [XmlRoot(nameof(PubSubChannelDefinition), Namespace = "http://santedb.org/pubsub")]
    [JsonObject]
    public class PubSubChannelDefinition : NonVersionedEntityData
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets whether the channel is active
        /// </summary>
        [XmlElement("active"), JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the endpoint
        /// </summary>
        [XmlElement("endpoint"), JsonProperty("endpoint")]
        public String Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the settings on this channel
        /// </summary>
        [XmlArray("settings"), XmlArrayItem("add"), JsonProperty("settings")]
        public List<PubSubChannelSetting> Settings { get; set; }

        /// <summary>
        /// Gets the dispatcher factory scheme
        /// </summary>
        [XmlElement("dispatcherFactoryId"), JsonProperty("dispatcherFactoryId")]
        public String DispatcherFactoryId { get; set; }
    }
}