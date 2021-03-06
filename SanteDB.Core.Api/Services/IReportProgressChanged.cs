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
    /// Represents a progress changing event
    /// </summary>
    public class ProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the progress
        /// </summary>
        public float Progress { get; set; }

        /// <summary>
        /// Display attached with the progress
        /// </summary>
        public Object State { get; set; }

        /// <summary>
        /// Progress changed event args
        /// </summary>
        public ProgressChangedEventArgs(float progress, object state)
        {
            this.Progress = progress;
            this.State = state;
        }
    }

    /// <summary>
    /// Represents a class which reports progress
    /// </summary>
    public interface IReportProgressChanged
    {

        /// <summary>
        /// Identifies that progress has changed
        /// </summary>
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
    }
}
