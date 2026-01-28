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
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Represents a service which can be used to manage the log files for the application instance
    /// </summary>
    public interface ILogManagerService : IServiceImplementation
    {


        /// <summary>
        /// Gets all log files from the specified logging source
        /// </summary>
        IEnumerable<FileInfo> GetLogFiles();

        /// <summary>
        /// Get the log file given the specified log identifier
        /// </summary>
        FileInfo GetLogFile(String logId);

        /// <summary>
        /// Delete or remove a log file from the infrastructure
        /// </summary>
        void DeleteLogFile(String logId);

    }
}
