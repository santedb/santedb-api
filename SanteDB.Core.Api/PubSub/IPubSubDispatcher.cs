using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Represents a dispatcher which can send discrete objects over a particular standard
    /// </summary>
    public interface IPubSubDispatcher
    {
        /// <summary>
        /// Gets the key for the channel
        /// </summary>
        Guid Key { get; }

        /// <summary>
        /// Gets the endpoint for the channel
        /// </summary>
        Uri Endpoint { get; }

        /// <summary>
        /// Gets the settings for the channel
        /// </summary>
        IDictionary<String, String> Settings { get; }

        void NotifyCreated<TModel>(TModel data);

        void NotifyUpdated<TModel>(TModel data);

        void NotifyObsoleted<TModel>(TModel data);

        void NotifyMerged<TModel>(TModel survivor, TModel[] subsumed);

        void NotifyUnMerged<TModel>(TModel primary, TModel[] unMerged);
    }
}
