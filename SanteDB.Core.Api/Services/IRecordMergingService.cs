using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which appropriately merges / unmerges records
    /// </summary>
    public interface IRecordMergingService<T>
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
