/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Core.Data
{
    /// <summary>
    /// Entity source which loads local objects
    /// </summary>
    public class PersistenceEntitySource : IEntitySourceProvider
    {


        #region IEntitySourceProvider implementation

        /// <summary>
        /// Get the persistence service source
        /// </summary>
        public TObject Get<TObject>(Guid? key) where TObject : IdentifiedData, new()
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<TObject>>();
            if (persistenceService != null && key.HasValue)
                return persistenceService.Get(key.Value, null, false, AuthenticationContext.Current.Principal);
            return default(TObject);
        }

        /// <summary>
        /// Get the specified version
        /// </summary>
        public TObject Get<TObject>(Guid? key, Guid? versionKey) where TObject : IdentifiedData, IVersionedEntity, new()
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<TObject>>();
            if (persistenceService != null && key.HasValue && versionKey.HasValue)
                return persistenceService.Get(key.Value, versionKey, false, AuthenticationContext.Current.Principal);
            else if (persistenceService != null && key.HasValue)
                return persistenceService.Get(key.Value, null, false, AuthenticationContext.SystemPrincipal);
            return default(TObject);
        }

        /// <summary>
        /// Get versioned relationships for the object
        /// </summary>
        public IEnumerable<TObject> GetRelations<TObject>(Guid? sourceKey, int? sourceVersionSequence) where TObject : IdentifiedData, IVersionedAssociation, new()
        {
            // Is the collection already loaded?
            return this.Query<TObject>(o => o.SourceEntityKey == sourceKey && o.ObsoleteVersionSequenceId != null);
        }

        /// <summary>
        /// Get versioned relationships for the object
        /// </summary>
        public IEnumerable<TObject> GetRelations<TObject>(Guid? sourceKey) where TObject : IdentifiedData, ISimpleAssociation, new()
        {
            // Is the collection already loaded?
            return this.Query<TObject>(o => o.SourceEntityKey == sourceKey);
        }

        /// <summary>
        /// Query the specified object
        /// </summary>
        public IEnumerable<TObject> Query<TObject>(Expression<Func<TObject, bool>> query) where TObject : IdentifiedData, new()
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IFastQueryDataPersistenceService<TObject>>();
            if (persistenceService != null)
            {
                var tr = 0;
                if (typeof(Act).IsAssignableFrom(typeof(TObject)) ||
                    typeof(ActParticipation).IsAssignableFrom(typeof(TObject)))
                    return persistenceService.QueryFast(query, Guid.Empty, 0, null, out tr, AuthenticationContext.Current.Principal);
                else
                    return persistenceService.Query(query, Guid.Empty, 0, null, out tr, AuthenticationContext.Current.Principal);

            }
            return new List<TObject>();
        }

        #endregion

    }
}

