﻿/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Daemon service which runs when the application is started
    /// </summary>
    public interface IDaemonService : IServiceImplementation
    {
        /// <summary>
        /// Starts the specified daemon service
        /// </summary>
        bool Start();
        /// <summary>
        /// Stop the service
        /// </summary>
        bool Stop();
        /// <summary>
        /// True when daemon is running
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// Fired when the daemon is starting
        /// </summary>
        event EventHandler Starting;
        /// <summary>
        /// Fired when the daemon is started
        /// </summary>
        event EventHandler Started;
        /// <summary>
        /// Fired when the daemon is stopping
        /// </summary>
        event EventHandler Stopping;
        /// <summary>
        /// Fired when the daemon has stopped
        /// </summary>
        event EventHandler Stopped;
    }
}
