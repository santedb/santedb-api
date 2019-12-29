﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-12-14
 */
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Represents a single performance counter
    /// </summary>
    public interface IDiagnosticsProbe
    {

        /// <summary>
        /// Gets the UUID of the performance counter
        /// </summary>
        [QueryParameter("id")]
        Guid Uuid { get; }

        /// <summary>
        /// Gets the name of the performance counter
        /// </summary>
        [QueryParameter("name")]
        string Name { get; }

        /// <summary>
        /// Gets the description of the performance counter
        /// </summary>
        [QueryParameter("description")]
        string Description { get; }

        /// <summary>
        /// Gets the current value of the performance counter
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the type of the performance counter
        /// </summary>
        Type Type { get; }

    }

    /// <summary>
    /// Represents a performance counter that returns a particular type of object
    /// </summary>
    /// <typeparam name="T">The type of the performance counter</typeparam>
    public interface IDiagnosticsProbe<T> : IDiagnosticsProbe
        where T : struct
    {

        /// <summary>
        /// Gets the current performance counter value
        /// </summary>
        new T Value { get; }

    }

    /// <summary>
    /// Represents a performance counter which is composed of other performance counters
    /// </summary>
    public interface ICompositeDiagnosticsProbe : IDiagnosticsProbe
    {

        /// <summary>
        /// Gets the value of the performance counter
        /// </summary>
        new IEnumerable<IDiagnosticsProbe> Value { get; }

    }
}