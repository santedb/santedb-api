using Newtonsoft.Json;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Data class for checkout lock
    /// </summary>
    [XmlRoot(nameof(ResourceCheckoutLock), Namespace = "http://santedb.org/model/lock")]
    [XmlType(nameof(ResourceCheckoutLock), Namespace = "http://santedb.org/model/lock")]
    public class ResourceCheckoutLock
    {
        /// <summary>
        /// Security user key which has the lock
        /// </summary>
        [XmlElement("user"), JsonProperty("user")]
        public String UserIdentity { get; set; }

        /// <summary>
        /// Lock start time
        /// </summary>
        [XmlElement("start"), JsonProperty("start")]
        public DateTimeOffset LockStart { get; set; }

        /// <summary>
        /// Lock expiration
        /// </summary>
        [XmlElement("expires"), JsonProperty("expires")]
        public DateTimeOffset LockExpiry { get; set; }
    }

    /// <summary>
    /// A checkout service which uses the current adhoc cache to manage checkouts
    /// </summary>
    public class CachedResourceCheckoutService : IResourceCheckoutService
    {
        // Cache service
        private IAdhocCacheService m_adhocCache;

        /// <summary>
        /// Create a new DI service instance
        /// </summary>
        public CachedResourceCheckoutService(IAdhocCacheService adhocCacheService)
        {
            this.m_adhocCache = adhocCacheService;
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Cached Checkout Service";

        /// <summary>
        /// Create cache key
        /// </summary>
        private String CreateCacheKey<T>(Guid key) => $"lock/{typeof(T).GetSerializationName()}/{key}";

        /// <summary>
        /// Check-in the resource
        /// </summary>
        public bool Checkin<T>(Guid key)
        {
            var resourceLock = this.m_adhocCache.Get<ResourceCheckoutLock>(this.CreateCacheKey<T>(key));
            if (resourceLock?.UserIdentity.Equals(AuthenticationContext.Current.Principal.Identity.Name, StringComparison.OrdinalIgnoreCase) == true) // Can unlock
            {
                return this.m_adhocCache.Remove(this.CreateCacheKey<T>(key));
            }
            else if (resourceLock != null) // cannot release another's lock
            {
                throw new ObjectLockedException(resourceLock.UserIdentity);
            }
            return true;
        }

        /// <summary>
        /// Checkout the object
        /// </summary>
        public bool Checkout<T>(Guid key)
        {
            var resourceLock = this.m_adhocCache.Get<ResourceCheckoutLock>(this.CreateCacheKey<T>(key));
            if (resourceLock == null || resourceLock.UserIdentity.Equals(AuthenticationContext.Current.Principal.Identity.Name, StringComparison.OrdinalIgnoreCase)) // Take the lock
            {
                resourceLock = new ResourceCheckoutLock()
                {
                    LockStart = DateTimeOffset.Now,
                    LockExpiry = DateTimeOffset.Now.AddMinutes(15),
                    UserIdentity = AuthenticationContext.Current.Principal.Identity.Name
                };
                this.m_adhocCache.Add(this.CreateCacheKey<T>(key), resourceLock, new TimeSpan(0, 15, 0));
                return true;
            }
            else
            {
                throw new ObjectLockedException(resourceLock.UserIdentity);
            }
        }
    }
}