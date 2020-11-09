using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Default notification relay service that scans the current appdomain for relays
    /// </summary>
    public class DefaultNotificationService : INotificationService
    {
        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DefaultNotificationService));

        // Relay cache
        private IDictionary<String, INotificationRelay> m_relays;

        /// <summary>
        /// Get all relays
        /// </summary>
        public IEnumerable<INotificationRelay> Relays => this.m_relays.Values;

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Notification Relay Service";

        /// <summary>
        /// Default notification service
        /// </summary>
        public DefaultNotificationService()
        {
            this.m_relays = ApplicationServiceContext.Current.GetService<IServiceManager>()
                .GetAllTypes()
                .Where(t => typeof(INotificationRelay).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(t => Activator.CreateInstance(t) as INotificationRelay)
                .ToDictionary(o => o.Scheme, o => o);
        }

        /// <summary>
        /// Get the specified notification relay
        /// </summary>
        public INotificationRelay GetNotificationRelay(Uri toAddress)
        {
            if (this.m_relays.TryGetValue(toAddress.Scheme, out INotificationRelay retVal))
                return retVal;
            return null;
        }

        /// <summary>
        /// Get notification relay
        /// </summary>
        public INotificationRelay GetNotificationRelay(string toAddress) => this.GetNotificationRelay(new Uri(toAddress));

        /// <summary>
        /// Send the specified data to the specified addressers
        /// </summary>
        public Guid[] Send(string[] to, string subject, string body, DateTimeOffset? scheduleDelivery = null, params NotificationAttachment[] attachments)
        {

            var sendRelays = to.Select(o => new Uri(o)).GroupBy(o => o.Scheme);
            List<Guid> retVal = new List<Guid>(to.Length);
            foreach(var itm in sendRelays)
            {
                if (this.m_relays.TryGetValue(itm.Key, out INotificationRelay relay))
                {
                    retVal.Add(relay.Send(itm.Select(o => o.ToString()).ToArray(), subject, body, scheduleDelivery, attachments));
                }
                else
                    this.m_tracer.TraceWarning("Cannot find relay on scheme {0}", itm.Key);
            }

            return retVal.ToArray();
        }
    }
}
