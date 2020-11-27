/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Model;
using System;

namespace SanteDB.Core.Event
{
    /// <summary>
    /// Represents notification event arguments.
    /// </summary>
    public class NotificationEventArgs<T> : EventArgs where T : IdentifiedData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationEventArgs{T}"/> class.
        /// </summary>
        public NotificationEventArgs()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationEventArgs{T}"/> class
        /// with identified data.
        /// </summary>
        /// <param name="data">The raw request data.</param>
        public NotificationEventArgs(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"{nameof(data)} cannot be null");
            }

            this.Data = data;
        }

        /// <summary>
        /// Gets or sets the data of the notification.
        /// </summary>
        public T Data { get; }
    }
}
