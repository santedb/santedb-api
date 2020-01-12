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
 * Date: 2019-8-8
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Features
{

    /// <summary>
    /// Represents a generic configuration option object which will cause any software 
    /// to instead use a type descritpor
    /// </summary>
    public class GenericFeatureConfiguration
    {

        /// <summary>
        /// Generic feature configuration
        /// </summary>
        public GenericFeatureConfiguration()
        {
            this.Options = new Dictionary<string, Func<Object>>();
            this.Values = new Dictionary<string, object>();
            this.Categories = new Dictionary<String, String[]>();
        }

        /// <summary>
        /// Gets the configuration options for this generic feature
        /// </summary>
        public Dictionary<String, Func<Object>> Options { get; set; }

        /// <summary>
        /// Gets the current set configuration values
        /// </summary>
        public Dictionary<String, Object> Values { get; set; }

        /// <summary>
        /// If the configuration is broken into categories
        /// </summary>
        public Dictionary<String, String[]> Categories { get; set; }
    }
}
