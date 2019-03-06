/*
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
 * User: justi
 * Date: 2019-1-12
 */
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which appropriately merges / unmerges records
    /// </summary>
    public interface IRecordMergingService<T> : IServiceImplementation
    {

        /// <summary>
        /// Merges the specified <paramref name="linkedDuplicates"/> into <paramref name="master"/>
        /// </summary>
        /// <param name="master">The master record to which the linked duplicates are to be attached</param>
        /// <param name="linkedDuplicates">The linked records to be merged to master</param>
        /// <returns>The newly updated master record</returns>
        T Merge(T master, IEnumerable<T> linkedDuplicates);

        /// <summary>
        /// Un-merges the specified <paramref name="unmergeDuplicate"/> from <paramref name="master"/>
        /// </summary>
        /// <param name="master">The master record from which a duplicate is to be removed</param>
        /// <param name="unmergeDuplicate">The record which is to be unmerged</param>
        /// <returns>The newly created master record from which <paramref name="unmergeDuplicate"/> was created</returns>
        T Unmerge(T master, T unmergeDuplicate);

    }
}
