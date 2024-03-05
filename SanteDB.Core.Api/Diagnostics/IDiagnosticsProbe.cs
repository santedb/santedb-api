/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Gets the units of the value
        /// </summary>
        String Unit { get; }

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
