/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Security;
using System;
using System.Security.Principal;
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
            if (resourceLock == null || 
                resourceLock.UserIdentity.Equals(AuthenticationContext.Current.Principal.Identity.Name, StringComparison.OrdinalIgnoreCase) ||
                resourceLock.LockExpiry < DateTimeOffset.Now) // Take the lock
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

        /// <summary>
        /// Returns true if the service is chedked out
        /// </summary>
        public bool IsCheckedout<T>(Guid key, out IIdentity currentOwner)
        {
            var resourceLock = this.m_adhocCache.Get<ResourceCheckoutLock>(this.CreateCacheKey<T>(key));
            if (resourceLock != null)
            {
                currentOwner = new GenericIdentity(resourceLock.UserIdentity);
                return true;
            }
            else
            {
                currentOwner = null;
                return false;
            }

        }
    }
}