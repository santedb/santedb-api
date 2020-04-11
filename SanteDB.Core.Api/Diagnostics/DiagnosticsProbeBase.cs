/*
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
 * Date: 2019-12-24
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets the value
        /// </summary>
        object IDiagnosticsProbe.Value => this.Value;
    }
}
