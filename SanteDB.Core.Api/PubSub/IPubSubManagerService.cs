using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Represents a pub/sub manager service
    /// </summary>
    public interface IPubSubManagerService : IServiceImplementation
    {

        /// <summary>
        /// Fired when a subscription is requested, but not yet registered
        /// </summary>
        event EventHandler<DataPersistingEventArgs<PubSubSubscriptionDefinition>> Subscribing;

        /// <summary>
        /// Fired after a subscription has been registered
        /// </summary>
        event EventHandler<DataPersistedEventArgs<PubSubSubscriptionDefinition>> Subscribed;

        /// <summary>
        /// Fired when an unsubscription is requested
        /// </summary>
        event EventHandler<DataPersistingEventArgs<PubSubSubscriptionDefinition>> UnSubscribing;

        /// <summary>
        /// Fired when a subscription has been terminated
        /// </summary>
        event EventHandler<DataPersistedEventArgs<PubSubSubscriptionDefinition>> UnSubscribed;

        /// <summary>
        /// Find an existing channel
        /// </summary>
        IEnumerable<PubSubChannelDefinition> FindChannel(Expression<Func<PubSubChannelDefinition, bool>> filter);

        /// <summary>
        /// Find an existing subscription
        /// </summary>
        IEnumerable<PubSubSubscriptionDefinition> FindSubscription(Expression<Func<PubSubSubscriptionDefinition, bool>> filter);

        /// <summary>
        /// Registers the specified pub-sub channel using the specified dispatcher
        /// </summary>
        PubSubChannelDefinition RegisterChannel(String name, Type channelType, Uri endpoint, IDictionary<String, String> settings);

        /// <summary>
        /// Register a new subscription for the specified type
        /// </summary>
        PubSubSubscriptionDefinition RegisterSubscription<TModel>(String name, PubSubEventType events, Expression<Func<TModel,bool>> filter, Guid channelId);

        /// <summary>
        /// Gets the channel information
        /// </summary>
        PubSubChannelDefinition GetChannel(Guid id);

        /// <summary>
        /// Removes the specified channel and all related subscriptions
        /// </summary>
        PubSubChannelDefinition RemoveChannel(Guid id);

        /// <summary>
        /// Removes the subscription 
        /// </summary>
        PubSubSubscriptionDefinition RemoveSubscription(Guid id);


    }
}
