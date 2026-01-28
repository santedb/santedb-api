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
using System;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Represents a performance counter base class
    /// </summary>
    public abstract class DiagnosticsProbeBase<TMeasure> : IDiagnosticsProbe<TMeasure>
        where TMeasure : struct
    {

        /// <summary>
        /// Base performance counter
        /// </summary>
        public DiagnosticsProbeBase(String name, String description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Get the value of the measure
        /// </summary>
        public abstract TMeasure Value { get; }

        /// <summary>
        /// Gets the identifier for the counter
        /// </summary>
        public abstract Guid Uuid { get; }

        /// <summary>
        /// Get the name of the measure
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the measure
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the type of the measure
        /// </summary>
        public Type Type => typeof(TMeasure);

        /// <summary>
        /// Gets the unit of measure
        /// </summary>
        public abstract String Unit { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        object IDiagnosticsProbe.Value => this.Value;
    }
}
