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
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;

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
        /// Notify that a subscription data object has been created
        /// </summary>
        /// <param name="data">The data that was created</param>
        void NotifyCreated<TModel>(TModel data) where TModel : IdentifiedData;

        /// <summary>
        /// Notify that a subscription data object has been updated
        /// </summary>
        /// <param name="data">The data that was updated</param>
        void NotifyUpdated<TModel>(TModel data) where TModel : IdentifiedData;

        /// <summary>
        /// Notify that a subscribed object was obsoleted
        /// </summary>
        /// <param name="data">The data which was obsoleted</param>
        void NotifyObsoleted<TModel>(TModel data) where TModel : IdentifiedData;

        /// <summary>
        /// Notify that a subscribed type was merged
        /// </summary>
        /// <param name="survivor">The record which survived the merge</param>
        /// <param name="subsumed">The record(s) which were consumed</param>
        void NotifyMerged<TModel>(TModel survivor, IEnumerable<TModel> subsumed) where TModel : IdentifiedData;

        /// <summary>
        /// Notify that a subscribed type was unmerged
        /// </summary>
        /// <param name="primary">The primary record which was unmerged (the remaining record)</param>
        /// <param name="unMerged">The records which were un-merged</param>
        void NotifyUnMerged<TModel>(TModel primary, IEnumerable<TModel> unMerged) where TModel : IdentifiedData;

        /// <summary>
        /// Notify subscribers that two objects have been linked together
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="target">The link that was established</param>
        /// <param name="primary">The primary data that was the focus of the link</param>
        void NotifyLinked<TModel>(TModel primary, TModel target) where TModel : IdentifiedData;


        /// <summary>
        /// Notify subscribers two objects being unlinked
        /// </summary>
        /// <typeparam name="TModel">The type of model</typeparam>
        /// <param name="holder">The holder of the relationship</param>
        /// <param name="target">The link that was removed</param>
        void NotifyUnlinked<TModel>(TModel holder, TModel target) where TModel : IdentifiedData;
    }
}