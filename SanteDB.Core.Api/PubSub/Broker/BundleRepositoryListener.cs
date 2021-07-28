using SanteDB.Core.Event;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// Chained bundle repository listener
    /// </summary>
    internal class BundleRepositoryListener : PubSubRepositoryListener<Bundle>
    {
        /// <summary>
        /// Bundle repository listener ctor
        /// </summary>
        public BundleRepositoryListener(IPubSubManagerService pubSubManager, IPersistentQueueService queueService, IServiceManager serviceManager) : base(pubSubManager, queueService, serviceManager)
        {

        }

        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnInserted(object sender, DataPersistedEventArgs<Bundle> e)
        {
            using (AuthenticationContext.EnterSystemContext())
            {
                foreach (var itm in e.Data.Item.Where(i => e.Data.FocalObjects.Contains(i.Key.Value)))
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                        dsptchr.NotifyCreated(itm);
                }
            }
        }

        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnSaved(object sender, DataPersistedEventArgs<Bundle> e)
        {
            using (AuthenticationContext.EnterSystemContext())
            {
                foreach (var itm in e.Data.Item.Where(i => e.Data.FocalObjects.Contains(i.Key.Value)))
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                        dsptchr.NotifyUpdated(itm);
                }
            }
        }

        /// <summary>
        /// Notify obsoleted
        /// </summary>
        protected override void OnObsoleted(object sender, DataPersistedEventArgs<Bundle> e)
        {
            using (AuthenticationContext.EnterSystemContext())
            {
                foreach (var itm in e.Data.Item.Where(i => e.Data.FocalObjects.Contains(i.Key.Value)))
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                        dsptchr.NotifyObsoleted(itm);
                }
            }
        }
    }
}
