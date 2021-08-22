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
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Event
{

    /// <summary>
    /// Data has been merged event args
    /// </summary>
    public class DataMergeEventArgs<TModel> : EventArgs
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Gets the master record
        /// </summary>
        public Guid SurvivorKey { get; }

        /// <summary>
        /// Gets the linked records
        /// </summary>
        public IEnumerable<Guid> LinkedKeys { get; }
        
        /// <summary>
        /// Creates a new data merge event args structure
        /// </summary>
        public DataMergeEventArgs(Guid master, IEnumerable<Guid> linked)
        {
            this.SurvivorKey = master;
            this.LinkedKeys = linked;
        }
    }

    /// <summary>
    /// Data will be merged event args
    /// </summary>
    public class DataMergingEventArgs<TModel> : DataMergeEventArgs<TModel>
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Set to true when the callee wishes to cancel the operation
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Creates a new data merge event args structure
        /// </summary>
        public DataMergingEventArgs(Guid masterKey, IEnumerable<Guid> linkedKeys) : base(masterKey, linkedKeys)
        {
        }
    }

}
