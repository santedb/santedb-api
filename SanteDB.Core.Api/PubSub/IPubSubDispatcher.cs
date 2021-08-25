/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Represents a dispatcher which can send discrete objects over a particular standard
    /// </summary>
    public interface IPubSubDispatcher
    {
        /// <summary>
        /// Gets the key for the channel
        /// </summary>
        Guid Key { get; }

        /// <summary>
        /// Gets the endpoint for the channel
        /// </summary>
        Uri Endpoint { get; }

        /// <summary>
        /// Gets the settings for the channel
        /// </summary>
        IDictionary<String, String> Settings { get; }

        /// <summary>
        /// Notify created
        /// </summary>
        void NotifyCreated<TModel>(TModel data) where TModel : IdentifiedData;

        void NotifyUpdated<TModel>(TModel data) where TModel : IdentifiedData;

        void NotifyObsoleted<TModel>(TModel data) where TModel : IdentifiedData;

        void NotifyMerged<TModel>(TModel survivor, TModel[] subsumed) where TModel : IdentifiedData;

        void NotifyUnMerged<TModel>(TModel primary, TModel[] unMerged) where TModel : IdentifiedData;
    }
}
