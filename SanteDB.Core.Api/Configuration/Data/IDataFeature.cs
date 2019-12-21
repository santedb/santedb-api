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

namespace SanteDB.Core.Configuration.Data
{

    /// <summary>
    /// Represents a particular feature for deployment
    /// </summary>
    public interface IDataFeature
    {
        /// <summary>
        /// Get the name of the data feature
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        String Description { get; }

        /// <summary>
        /// Gets the database provider for which the feature is intended
        /// </summary>
        String InvariantName { get; }

        /// <summary>
        /// Get the SQL required to deploy the feature
        /// </summary>
        String GetDeploySql();

        /// <summary>
        /// Get SQL required to determine if feature is installed
        /// </summary>
        String GetCheckSql();
    }
}
