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

using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Event arguments for process changing event
    /// </summary>
    /// <remarks>This class allows the <see cref="IReportProgressChanged"/> class to notify listeners of the current
    /// status of an operation it is performing.</remarks>
    public class ProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the progress of the event
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Contextual information (such as text) which is appended to the progress on user interfaces or REST messages
        /// </summary>
        public Object State { get; }

        /// <summary>
        /// Creates a new progress changed event
        /// </summary>
        /// <param name="progress">The progress of the operation which is being reported on</param>
        /// <param name="state">The state object to report with the progress</param>
        public ProgressChangedEventArgs(float progress, object state)
        {
            this.Progress = progress;
            this.State = state;
        }
    }

    /// <summary>
    /// Defines a class that can report progress has changed over a long running process
    /// </summary>
    public interface IReportProgressChanged
    {
        /// <summary>
        /// Fired when the progress of this instance has changed
        /// </summary>
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
    }
}