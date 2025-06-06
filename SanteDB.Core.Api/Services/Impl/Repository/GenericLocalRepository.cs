﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Represents a base class for entity repository services
    /// </summary>
    [ServiceProvider("Local Repository Service", Dependencies = new Type[] { typeof(IDataPersistenceService) })]
    public class GenericLocalRepository<TEntity> :
        IRepositoryService,
        IValidatingRepositoryService<TEntity>,
        IRepositoryService<TEntity>,
        INotifyRepositoryService<TEntity>,
        IRepositoryServiceEx<TEntity>,
        ISecuredRepositoryService,
        ILocalServiceProvider<IRepositoryService<TEntity>>
        where TEntity : IdentifiedData
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => $"Local repository service for {typeof(TEntity).FullName}";

        /// <summary>
        /// Trace source
        /// </summary>
        protected readonly Tracer m_traceSource = Tracer.GetTracer(typeof(GenericLocalRepository<TEntity>));

        /// <summary>
        /// Fired prior to inserting the record
        /// </summary>
        public event EventHandler<DataPersistingEventArgs<TEntity>> Inserting;

        /// <summary>
        /// Fired after the record has been inserted
        /// </summary>
        public event EventHandler<DataPersistedEventArgs<TEntity>> Inserted;

        /// <summary>
        /// Fired before the record is saved
        /// </summary>
        public event EventHandler<DataPersistingEventArgs<TEntity>> Saving;

        /// <summary>
        /// Fired after the record has been persisted
        /// </summary>
        public event EventHandler<DataPersistedEventArgs<TEntity>> Saved;

        /// <summary>
        /// Fired prior to the record being retrieved
        /// </summary>
        public event EventHandler<DataRetrievingEventArgs<TEntity>> Retrieving;

        /// <summary>
        /// Fired after the record has been retrieved
        /// </summary>
        public event EventHandler<DataRetrievedEventArgs<TEntity>> Retrieved;

        /// <summary>
        /// Fired before a query is executed
        /// </summary>
        public event EventHandler<QueryRequestEventArgs<TEntity>> Querying;

        /// <summary>
        /// Fired after query results have been executed
        /// </summary>
        public event EventHandler<QueryResultEventArgs<TEntity>> Queried;

        /// <summary>
        /// Data is obsoleting
        /// </summary>
        public event EventHandler<DataPersistingEventArgs<TEntity>> Deleting;

        /// <summary>
        /// Data has obsoleted
        /// </summary>
        public event EventHandler<DataPersistedEventArgs<TEntity>> Deleted;

        /// <summary>
        /// Gets the policy required for querying
        /// </summary>
        protected virtual String QueryPolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <summary>
        /// Gets the policy required for reading
        /// </summary>
        protected virtual String ReadPolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <summary>
        /// Gets the policy required for writing
        /// </summary>
        protected virtual String WritePolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <summary>
        /// Gets the policy required for deleting
        /// </summary>
        protected virtual String DeletePolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <summary>
        /// Gets the policy for altering
        /// </summary>
        protected virtual String AlterPolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <inheritdoc/>
        public IRepositoryService<TEntity> LocalProvider => this;

        // Privacy service
        private IPrivacyEnforcementService m_privacyService;

        /// <summary>
        /// Reference to the policy enforcement service
        /// </summary>
        protected IPolicyEnforcementService m_policyService;

        /// <summary>
        /// Data persistence services
        /// </summary>
        protected IDataPersistenceService<TEntity> m_dataPersistenceService;

        /// <summary>
        /// Creates a new generic local repository with specified privacy service
        /// </summary>
        public GenericLocalRepository(IPolicyEnforcementService policyService, IDataPersistenceService<TEntity> dataPersistence, IPrivacyEnforcementService privacyService = null)
        {
            this.m_privacyService = privacyService;
            this.m_policyService = policyService;
            this.m_dataPersistenceService = dataPersistence;
        }

        /// <summary>
        /// Performs insert of object
        /// </summary>
        public virtual TEntity Insert(TEntity data)
        {

            if (data == default(TEntity))
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Demand permission
            this.DemandWrite(data);

            // Validate the resource
            data = this.Validate(data);

            // Call privacy hook
            if (this.m_privacyService?.ValidateWrite(data, AuthenticationContext.Current.Principal) == false)
            {
                this.ThrowPrivacyValidationException(data);
            }

            // Fire pre-persistence triggers
            var prePersistence = new DataPersistingEventArgs<TEntity>(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            this.Inserting?.Invoke(this, prePersistence);
            if (prePersistence.Cancel)
            {
                this.m_traceSource.TraceInfo("Pre-persistence event signal cancel: {0}", data);

                // Fired inserted trigger
                if (prePersistence.Success)
                {
                    this.Inserted?.Invoke(this, new DataPersistedEventArgs<TEntity>(prePersistence.Data, TransactionMode.Commit, AuthenticationContext.Current.Principal));
                }
                return this.m_privacyService?.Apply(prePersistence.Data, AuthenticationContext.Current.Principal) ?? prePersistence.Data;
            }

            // Did the pre-persistence service change the type to a batch
            var businessRulesService = ApplicationServiceContext.Current.GetBusinessRulesService<TEntity>();

            var persistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<TEntity>>();
            data = businessRulesService?.BeforeInsert(data) ?? prePersistence.Data;
            data = persistenceService.Insert(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            businessRulesService?.AfterInsert(data);
            this.Inserted?.Invoke(this, new DataPersistedEventArgs<TEntity>(data, TransactionMode.Commit, AuthenticationContext.Current.Principal));
            return this.m_privacyService?.Apply(data, AuthenticationContext.Current.Principal) ?? data;
        }

        /// <summary>
        /// Throw a privacy validation exception
        /// </summary>
        private void ThrowPrivacyValidationException(TEntity data)
        {
            throw new DetectedIssueException(
                new DetectedIssue(DetectedIssuePriorityType.Error, "privacy", ErrorMessages.PRIVACY_VIOLATION_DETECTED, DetectedIssueKeys.PrivacyIssue, data.ToString())
            );
        }

        /// <summary>
        /// Delete the specified data
        /// </summary>
        public virtual TEntity Delete(Guid key)
        {
            // Demand permission
            this.DemandDelete(key);

            var entity = this.m_dataPersistenceService.Get(key, null, AuthenticationContext.Current.Principal);

            if (entity == null)
            {
                throw new KeyNotFoundException(key.ToString());
            }

            // Fire pre-persistence triggers
            var prePersistence = new DataPersistingEventArgs<TEntity>(entity, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            this.Deleting?.Invoke(this, prePersistence);
            if (prePersistence.Cancel)
            {
                this.m_traceSource.TraceInfo("Pre-persistence event signal cancel obsolete: {0}", key);
                // Fired inserted trigger
                if (prePersistence.Success)
                {
                    this.Deleted?.Invoke(this, new DataPersistedEventArgs<TEntity>(prePersistence.Data, TransactionMode.Commit, AuthenticationContext.Current.Principal));
                }
                return this.m_privacyService?.Apply(prePersistence.Data, AuthenticationContext.Current.Principal) ?? prePersistence.Data;
            }

            var businessRulesService = ApplicationServiceContext.Current.GetBusinessRulesService<TEntity>();

            entity = businessRulesService?.BeforeDelete(entity) ?? entity;
            entity = this.m_dataPersistenceService.Delete(entity.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            entity = businessRulesService?.AfterDelete(entity) ?? entity;

            this.Deleted?.Invoke(this, new DataPersistedEventArgs<TEntity>(entity, TransactionMode.Commit, AuthenticationContext.Current.Principal));

            return this.m_privacyService?.Apply(entity, AuthenticationContext.Current.Principal) ?? entity;
        }

        /// <summary>
        /// Get the specified key
        /// </summary>
        public virtual TEntity Get(Guid key)
        {
            return this.Get(key, Guid.Empty);
        }

        /// <summary>
        /// Get specified data from persistence
        /// </summary>
        public virtual TEntity Get(Guid key, Guid versionKey)
        {
            // Demand permission
            this.DemandRead(key);


            var businessRulesService = ApplicationServiceContext.Current.GetBusinessRulesService<TEntity>();

            var preRetrieve = new DataRetrievingEventArgs<TEntity>(key, versionKey, AuthenticationContext.Current.Principal);

            this.Retrieving?.Invoke(this, preRetrieve);
            if (preRetrieve.Cancel)
            {
                this.m_traceSource.TraceInfo("Pre-retrieve trigger signals cancel: {0}", key);
                return this.m_privacyService?.Apply(preRetrieve.Result, AuthenticationContext.Current.Principal) ?? preRetrieve.Result;
            }

            var result = this.m_dataPersistenceService.Get(key, versionKey, AuthenticationContext.Current.Principal);
            var retVal = businessRulesService?.AfterRetrieve(result) ?? result;
            var postEvt = new DataRetrievedEventArgs<TEntity>(retVal, AuthenticationContext.Current.Principal);
            this.Retrieved?.Invoke(this, postEvt);

            return this.m_privacyService?.Apply(postEvt.Data, AuthenticationContext.Current.Principal) ?? postEvt.Data;
        }

        /// <summary>
        /// Save the specified entity (INSERT OR IGNORE)
        /// </summary>
        public virtual TEntity Save(TEntity data)
        {

            if (data == default(TEntity))
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Demand permission
            this.DemandAlter(data);

            data = this.Validate(data);

            var businessRulesService = ApplicationServiceContext.Current.GetBusinessRulesService<TEntity>();

            try
            {
                if (this.m_privacyService?.ValidateWrite(data, AuthenticationContext.Current.Principal) == false)
                {
                    this.ThrowPrivacyValidationException(data);
                }

                var preSave = new DataPersistingEventArgs<TEntity>(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                this.Saving?.Invoke(this, preSave);
                if (preSave.Cancel)
                {
                    this.m_traceSource.TraceInfo("Persistence layer indicates pre-save cancel: {0}", data);
                    // Fired inserted trigger
                    if (preSave.Success)
                    {
                        this.Saved?.Invoke(this, new DataPersistedEventArgs<TEntity>(preSave.Data, TransactionMode.Commit, AuthenticationContext.Current.Principal));
                    }
                    return this.m_privacyService?.Apply(preSave.Data, AuthenticationContext.Current.Principal) ?? preSave.Data;
                }
                else
                {
                    data = preSave.Data; // Data may have been updated
                }

                var currentObject = data is IResourceCollection ? null : this.m_dataPersistenceService.Get(data.Key.GetValueOrDefault(), null, AuthenticationContext.Current.Principal);
                if (data.Key.HasValue && currentObject != null)
                {
                    data = businessRulesService?.BeforeUpdate(data) ?? data;
                    data = this.m_dataPersistenceService.Update(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                    businessRulesService?.AfterUpdate(data);
                    this.Saved?.Invoke(this, new DataPersistedOriginalEventArgs<TEntity>(data, currentObject, TransactionMode.Commit, AuthenticationContext.Current.Principal));
                }
                else
                {
                    data = businessRulesService?.BeforeInsert(data) ?? data;
                    data = this.m_dataPersistenceService.Insert(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                    businessRulesService?.AfterInsert(data);
                    this.Saved?.Invoke(this, new DataPersistedEventArgs<TEntity>(data, TransactionMode.Commit, AuthenticationContext.Current.Principal));
                }

                return this.m_privacyService?.Apply(data, AuthenticationContext.Current.Principal) ?? data;
            }
            catch (KeyNotFoundException)
            {
                return this.Insert(data);
            }
        }

        /// <summary>
        /// Validate a patient before saving
        /// </summary>
        public virtual TEntity Validate(TEntity p)
        {
            var businessRulesService = ApplicationServiceContext.Current.GetBusinessRulesService<TEntity>();

            var details = businessRulesService?.Validate(p) ?? new List<DetectedIssue>();

            if (details.Any(d => d.Priority == DetectedIssuePriorityType.Error))
            {
                throw new DetectedIssueException(details);
            }

            // Bundles cascade
            if (p is Bundle bundle)
            {
                for (int i = 0; i < bundle.Item.Count; i++)
                {
                    var itm = bundle.Item[i];

                    if (!(itm is IdentifiedDataReference))
                    {
                        var vrst = typeof(IValidatingRepositoryService<>).MakeGenericType(itm.GetType());
                        var vrsi = ApplicationServiceContext.Current.GetService(vrst);

                        if (vrsi != null)
                        {
                            bundle.Item[i] = vrsi.GetType().GetMethod(nameof(Validate)).Invoke(vrsi, new object[] { itm }) as IdentifiedData;
                        }
                    }
                }
            }
            return p;
        }

        /// <summary>
        /// Perform a simple find
        /// </summary>
        public virtual IQueryResultSet<TEntity> Find(Expression<Func<TEntity, bool>> query)
        {
            // Demand permission
            this.DemandQuery();

            var businessRulesService = ApplicationServiceContext.Current.GetBusinessRulesService<TEntity>();

            // Notify query
            var preQueryEventArgs = new QueryRequestEventArgs<TEntity>(query, AuthenticationContext.Current.Principal);
            this.Querying?.Invoke(this, preQueryEventArgs);
            IQueryResultSet<TEntity> results = null;
            if (preQueryEventArgs.Cancel) // Cancel the request
            {
                results = preQueryEventArgs.Results;
            }
            else
            {
                results = this.m_dataPersistenceService.Query(preQueryEventArgs.Query, AuthenticationContext.Current.Principal);
            }

            // 1. Let the BRE run
            var retVal = businessRulesService != null ? businessRulesService.AfterQuery(results) : results;

            // 2. Broadcast query performed
            var postEvt = new QueryResultEventArgs<TEntity>(query, retVal, AuthenticationContext.AnonymousPrincipal);
            this.Queried?.Invoke(this, postEvt);

            // 2. Apply Filters if needed
            retVal = this.m_privacyService?.Apply(retVal, AuthenticationContext.Current.Principal) ?? retVal;

            return retVal;
        }

        /// <summary>
        /// Perform a normal find
        /// </summary>
        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> query, int offset, int? count, out int totalResults, params ModelSort<TEntity>[] orderBy)
        {
            var results = this.Find(query);
            totalResults = results.Count();

            if (results is IOrderableQueryResultSet<TEntity> orderable)
            {
                foreach (var itm in orderBy)
                {
                    if (itm.SortOrder == SanteDB.Core.Model.Map.SortOrderType.OrderBy)
                    {
                        results = orderable.OrderBy(itm.SortProperty);
                    }
                    else
                    {
                        results = orderable.OrderByDescending(itm.SortProperty);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Demand write permission
        /// </summary>
        public virtual void DemandWrite(object data)
        {
            this.m_policyService.Demand(this.WritePolicy);
        }

        /// <summary>
        /// Demand read
        /// </summary>
        /// <param name="key"></param>
        public virtual void DemandRead(Guid key)
        {
            this.m_policyService.Demand(this.ReadPolicy);
        }

        /// <summary>
        /// Demand delete permission
        /// </summary>
        public virtual void DemandDelete(Guid key)
        {
            this.m_policyService.Demand(this.DeletePolicy);
        }

        /// <summary>
        /// Demand alter permission
        /// </summary>
        public virtual void DemandAlter(object data)
        {
            this.m_policyService.Demand(this.AlterPolicy);
        }

        /// <summary>
        /// Demand query
        /// </summary>
        public virtual void DemandQuery()
        {
            this.m_policyService.Demand(this.QueryPolicy);
        }

        /// <summary>
        /// Get the specified data
        /// </summary>
        IdentifiedData IRepositoryService.Get(Guid key)
        {
            return this.Get(key);
        }

        /// <summary>
        /// Find specified data
        /// </summary>
        IQueryResultSet IRepositoryService.Find(Expression query)
        {
            if (query is Expression<Func<TEntity, bool>> qe)
            {
                return this.Find(qe);
            }
            else
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.INVALID_EXPRESSION_TYPE, typeof(Expression<Func<TEntity, bool>>), query.GetType()));
            }
        }

        /// <summary>
        /// Find specified data
        /// </summary>
        IEnumerable<IdentifiedData> IRepositoryService.Find(Expression query, int offset, int? count, out int totalResults)
        {
            return this.Find((Expression<Func<TEntity, bool>>)query, offset, count, out totalResults).OfType<IdentifiedData>();
        }

        /// <summary>
        /// Insert the specified data
        /// </summary>
        IdentifiedData IRepositoryService.Insert(object data)
        {
            return this.Insert((TEntity)data);
        }

        /// <summary>
        /// Save specified data
        /// </summary>
        IdentifiedData IRepositoryService.Save(object data)
        {
            return this.Save((TEntity)data);
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        IdentifiedData IRepositoryService.Delete(Guid key)
        {
            return this.Delete(key);
        }

        /// <summary>
        /// Touch the specified object
        /// </summary>
        public void Touch(Guid key)
        {
            if (this.m_dataPersistenceService is IDataPersistenceServiceEx<TEntity> ide)
            {
                ide.Touch(key, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}