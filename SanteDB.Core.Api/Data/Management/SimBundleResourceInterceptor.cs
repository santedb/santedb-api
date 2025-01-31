/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Management
{
    /// <summary>
    /// Represents a merging service that redirects bundle persistence to one or more <see cref="SimResourceInterceptor{TModel}"/>
    /// </summary>
    internal class SimBundleResourceInterceptor : IDisposable
    {
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(SimBundleResourceInterceptor));

        // Listeners for chaining
        private readonly IEnumerable<ISimResourceInterceptor> m_listeners;

        // Notify repository
        private readonly INotifyRepositoryService<Bundle> m_notifyRepository;
        // Bundle Persistence
        private readonly IDataPersistenceService<Bundle> m_bundlePersistence;
        // BRE
        private readonly IBusinessRulesService<Bundle> m_businessRulesService;

        /// <summary>
        /// Bundle resource listener
        /// </summary>
        public SimBundleResourceInterceptor(IEnumerable<ISimResourceInterceptor> listeners)
        {
            if (listeners == null)
            {
                throw new ArgumentNullException(nameof(listeners), "Listeners for chained invokation is required");
            }
            this.m_listeners = listeners;

            foreach (var itm in this.m_listeners)
            {
                this.m_tracer.TraceInfo("Bundles will be chained to {0}", itm.GetType().FullName);
            }

            this.m_notifyRepository = ApplicationServiceContext.Current.GetService<INotifyRepositoryService<Bundle>>();
            this.m_bundlePersistence = ApplicationServiceContext.Current.GetService<IDataPersistenceService<Bundle>>();
            this.m_businessRulesService = ApplicationServiceContext.Current.GetService<IBusinessRulesService<Bundle>>();
            // Subscribe
            this.m_notifyRepository.Inserting += this.OnSaving;
            this.m_notifyRepository.Saving += this.OnSaving;
            this.m_notifyRepository.Deleted += this.OnDeleted;
        }

        /// <summary>
        /// Get the SIM resource handler for <paramref name="dataElement"/>
        /// </summary>
        private ISimResourceInterceptor GetResourceInterceptor(IdentifiedData dataElement)
        {
            if (dataElement == null)
            {
                throw new ArgumentNullException(nameof(dataElement));
            }

            var deType = dataElement.GetType();
            if (typeof(IHasClassConcept).IsAssignableFrom(deType) && typeof(IHasRelationships).IsAssignableFrom(deType))
            {
                var dType = typeof(SimResourceInterceptor<>).MakeGenericType(deType);
                return this.m_listeners.FirstOrDefault(o => o.GetType() == dType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Chain invoke a bundle
        /// </summary>
        protected void OnSaving(object sender, DataPersistingEventArgs<Bundle> e)
        {
            e.Cancel = e.Data.Item.Any(o => this.GetResourceInterceptor(o) != null);
            if (e.Cancel)
            {
                e.Data.Item.RemoveAll(itm => itm is EntityRelationship er && er.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && !er.NegationIndicator.GetValueOrDefault() ||
                    itm is ActRelationship ar && ar.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && !ar.NegationIndicator.GetValueOrDefault());
                var persistenceBundle = new Bundle()
                {
                    CorrelationKey = e.Data.CorrelationKey,
                    CorrelationSequence = e.Data.CorrelationSequence,
                    FocalObjects = e.Data.FocalObjects
                };
                foreach (var itm in e.Data.Item)
                {
                    var handler = this.GetResourceInterceptor(itm);
                    if (handler == null)
                    {
                        persistenceBundle.Add(itm);
                    }
                    else
                    {
                        persistenceBundle.AddRange(handler.DoMatchingLogic(itm));
                        if (!persistenceBundle.Item.Any(i=>i.SemanticEquals(itm)))
                        {
                            persistenceBundle.Add(itm);
                        }
                    }
                }

                
                e.Data = this.m_businessRulesService?.BeforeInsert(persistenceBundle) ?? persistenceBundle;
                e.Data = this.m_bundlePersistence.Insert(e.Data, e.Mode, AuthenticationContext.Current.Principal);
                e.Data = this.m_businessRulesService?.AfterInsert(e.Data) ?? persistenceBundle;
                e.Success = true;
            }
        }

        /// <summary>
        /// On obsoleting
        /// </summary>
        protected void OnDeleted(object sender, DataPersistedEventArgs<Bundle> e)
        {
            var deletionBundle = new Bundle()
            {
                CorrelationKey = e.Data.CorrelationKey,
                CorrelationSequence = e.Data.CorrelationSequence
            };

            foreach (var itm in e.Data.Item.Where(i => i.BatchOperation == Model.DataTypes.BatchOperationType.Delete))
            {
                var handler = this.GetResourceInterceptor(itm);
                if (handler != null)
                {
                    deletionBundle.AddRange(handler.DoDeletionLogic(itm));
                }
            }
            this.m_bundlePersistence.Update(deletionBundle, e.Mode, AuthenticationContext.SystemPrincipal);
        }

        /// <summary>
        /// Dispose of this object
        /// </summary>
        public virtual void Dispose()
        {
            if (this.m_notifyRepository != null)
            {
                this.m_notifyRepository.Inserting -= this.OnSaving;
                this.m_notifyRepository.Saving -= this.OnSaving;
                this.m_notifyRepository.Deleted -= this.OnDeleted;
            }
        }
    }
}