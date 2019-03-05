﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a data persistence service where arbitrary SQL can be run
    /// </summary>
    public interface ISqlDataPersistenceService : IServiceImplementation
    {

        /// <summary>
        /// Text that identifies the type of database system that is running
        /// </summary>
        string InvariantName { get; }

        /// <summary>
        /// Executes the arbitrary SQL
        /// </summary>
        void Execute(String sql);
    }
}
