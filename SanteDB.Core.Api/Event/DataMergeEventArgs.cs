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
        public TModel Master { get; }

        /// <summary>
        /// Gets the linked records
        /// </summary>
        public IEnumerable<TModel> Linked { get; }
        
        /// <summary>
        /// Creates a new data merge event args structure
        /// </summary>
        public DataMergeEventArgs(TModel master, IEnumerable<TModel> linked)
        {
            this.Master = master;
            this.Linked = linked;
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
        public DataMergingEventArgs(TModel master, IEnumerable<TModel> linked) : base(master, linked)
        {
        }
    }

}
