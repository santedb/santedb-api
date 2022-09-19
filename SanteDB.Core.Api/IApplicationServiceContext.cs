/*
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
 * Date: 2022-5-30
 */
using System;

namespace SanteDB.Core
{
    /// <summary>
    /// Represents an application service context
    /// </summary>
    public interface IApplicationServiceContext : IServiceProvider
    {
        /// <summary>
        /// Fired when the service context is starting
        /// </summary>
        event EventHandler Starting;

        /// <summary>
        /// Fired when the service context is started
        /// </summary>
        event EventHandler Started;

        /// <summary>
        /// Fired when the service is stopping
        /// </summary>
        event EventHandler Stopping;

        /// <summary>
        /// Fired when the service has stopped
        /// </summary>
        event EventHandler Stopped;

        /// <summary>
        /// Get whether the service is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Type of application hosting this SanteDB
        /// </summary>
        SanteDBHostType HostType { get; }

        /// <summary>
        /// Gets the start time
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Start the application context
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the service host context
        /// </summary>
        void Stop();

    }
}