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
using System.Collections.Generic;

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
            this.Options = new Dictionary<string, Func<object>>();
            this.Values = new Dictionary<string, object>();
            this.Categories = new Dictionary<string, string[]>();
        }

	    /// <summary>
        /// If the configuration is broken into categories
        /// </summary>
        public Dictionary<string, string[]> Categories { get; set; }

	    /// <summary>
        /// Gets the configuration options for this generic feature
        /// </summary>
        public Dictionary<string, Func<object>> Options { get; set; }

	    /// <summary>
        /// Gets the current set configuration values
        /// </summary>
        public Dictionary<string, object> Values { get; set; }
    }
}
