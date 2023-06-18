/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Security;
using System;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Tag persistence service for tags - is used as a fallback as it is slower than using the ADO or remote tag service
    /// </summary>
    [ServiceProvider("Local Tag Persistence"), Obsolete]
    public class LocalTagPersistenceService : ITagPersistenceService
    {
        private IDataCachingService m_cacheService;
        private readonly IDataPersistenceService<ActTag> m_actTagService;
        private readonly IDataPersistenceService<EntityTag> m_entityTagService;
        private readonly IDataPersistenceService<Act> m_actService;
        private readonly IDataPersistenceService<Entity> m_entityService;

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Local Tag Persistence";

        /// <summary>
        /// Create new local tag persistence service
        /// </summary>
        public LocalTagPersistenceService(IDataPersistenceService<ActTag> actTagPersistenceService,
            IDataPersistenceService<EntityTag> entityTagSerivce,
            IDataPersistenceService<Act> actService,
            IDataPersistenceService<Entity> entityService,
            IDataCachingService cacheService = null)
        {
            this.m_cacheService = cacheService;
            this.m_actTagService = actTagPersistenceService;
            this.m_entityTagService = entityTagSerivce;
            this.m_actService = actService;
            this.m_entityService = entityService;
        }

        /// <summary>
        /// Save tag
        /// </summary>
        public void Save(Guid sourceKey, ITag tag)
        {
            this.Save(sourceKey, tag.TagKey, tag.Value);
        }

        /// <inheritdoc/>
        public void Save(Guid sourceKey, String tagName, String tagValue)
        {
            using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete))
            {
                // Don't persist empty tags
                if (tagName.StartsWith("$"))
                {
                    return;
                }


                if (this.m_entityService.Query(o => o.Key == sourceKey, AuthenticationContext.SystemPrincipal).Any())
                {
                    var existing = this.m_entityTagService.Query(o => o.SourceEntityKey == sourceKey && o.TagKey == tagName, AuthenticationContext.Current.Principal).FirstOrDefault();
                    if (existing != null)
                    {
                        existing.Value = tagValue;
                        if (tagValue == null)
                        {
                            this.m_entityTagService.Delete(existing.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                        }
                        else
                        {
                            this.m_entityTagService.Update(existing as EntityTag, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                        }
                    }
                    else
                    {
                        this.m_entityTagService.Insert(new EntityTag(tagName, tagValue), TransactionMode.Commit, AuthenticationContext.Current.Principal);
                    }
                }
                else if (this.m_actService.Query(o => o.Key == sourceKey, AuthenticationContext.SystemPrincipal).Any())
                {
                    var existing = this.m_actTagService.Query(o => o.SourceEntityKey == sourceKey && o.TagKey == tagName, AuthenticationContext.Current.Principal).FirstOrDefault();
                    if (existing != null)
                    {
                        if (tagValue == null)
                        {
                            this.m_actTagService.Delete(existing.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                        }
                        else
                        {
                            existing.Value = tagValue;
                            this.m_actTagService.Update(existing, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                        }
                    }
                    else
                    {

                        this.m_actTagService.Insert(new ActTag(tagName, tagValue), TransactionMode.Commit, AuthenticationContext.Current.Principal);
                    }
                }

                this.m_cacheService?.Remove(sourceKey);
            }
        }
    }
}