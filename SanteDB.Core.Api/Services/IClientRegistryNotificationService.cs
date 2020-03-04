﻿/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
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
using SanteDB.Core.Event;
using SanteDB.Core.Model.Roles;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a client registry notification service.
    /// </summary>
    public interface IClientRegistryNotificationService : IServiceImplementation
    {
        /// <summary>
        /// Notify that duplicates have been resolved.
        /// </summary>
        /// <param name="eventArgs">The notification event arguments.</param>
        void NotifyDuplicatesResolved(NotificationEventArgs<Patient> eventArgs);

        /// <summary>
        /// Notify that a registration occurred.
        /// </summary>
        /// <param name="eventArgs">The notification event arguments.</param>
        void NotifyRegister(NotificationEventArgs<Patient> eventArgs);

        /// <summary>
        /// Notify that an update occurred.
        /// </summary>
        /// <param name="eventArgs">The notification event arguments.</param>
        void NotifyUpdate(NotificationEventArgs<Patient> eventArgs);
    }
}