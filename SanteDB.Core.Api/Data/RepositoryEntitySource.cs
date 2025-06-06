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
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.Data
{
    /// <summary>
    /// Entity source which loads objects using the <see cref="IRepositoryService{TModel}"/> instead of <see cref="IDataPersistenceService{TData}"/>
    /// </summary>
    public class RepositoryEntitySource : IEntitySourceProvider
    {
        /// <summary>
        /// Creates a new persistence entity source
        /// </summary>
        public RepositoryEntitySource()
        {
        }

        #region IEntitySourceProvider implementation

        /// <summary>
        /// Get the persistence service source
        /// </summary>
        public TObject Get<TObject>(Guid? key) where TObject : IdentifiedData, new()
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IRepositoryService<TObject>>();
            if (persistenceService != null && key.HasValue)
            {
                return persistenceService.Get(key.Value);
            }

            return default(TObject);
        }


        /// <summary>
        /// Get the specified version
        /// </summary>
        public TObject Get<TObject>(Guid? key, Guid? versionKey) where TObject : IdentifiedData, new()
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IRepositoryService<TObject>>();
            if (persistenceService != null && key.HasValue && versionKey.HasValue)
            {
                return persistenceService.Get(key.Value, versionKey.Value);
            }

            if (persistenceService != null && key.HasValue)
            {
                return persistenceService.Get(key.Value);
            }

            return default(TObject);
        }

        /// <summary>
        /// Get versioned relationships for the object
        /// </summary>
        public IQueryResultSet<TObject> GetRelations<TObject>(Guid? sourceKey, int? sourceVersionSequence) where TObject : IdentifiedData, IVersionedAssociation, new()
        {
            return this.Query<TObject>(o => o.SourceEntityKey == sourceKey && o.ObsoleteVersionSequenceId != null);
        }

        /// <summary>
        /// Get versioned relationships for the object
        /// </summary>
        public IQueryResultSet<TObject> GetRelations<TObject>(params Guid?[] sourceKey) where TObject : IdentifiedData, ISimpleAssociation, new()
        {
            return this.Query<TObject>(o => sourceKey.Contains(o.SourceEntityKey));
        }

        /// <inheritdoc/>
        public IQueryResultSet GetRelations(Type relatedType, params Guid?[] sourceKey)
        {
            var persistenceService = ApplicationServiceContext.Current.GetService(typeof(IRepositoryService<>).MakeGenericType(relatedType)) as IRepositoryService;
            var parm = Expression.Parameter(relatedType);
            var containsMethod = typeof(Enumerable).GetGenericMethod(nameof(Enumerable.Contains), new Type[] { typeof(Guid?) }, new Type[] { typeof(IEnumerable<Guid?>), typeof(Guid?) }) as System.Reflection.MethodInfo;

            Expression expr = Expression.Lambda(Expression.Call(null, containsMethod, Expression.Constant(sourceKey), Expression.MakeMemberAccess(parm, relatedType.GetProperty(nameof(ISimpleAssociation.SourceEntityKey)))), parm);
            return persistenceService?.Find(expr) ?? new MemoryQueryResultSet(new Object[0]);
        }

        /// <inheritdoc/>
        public IQueryResultSet GetInverseRelations(Type relatedType, params Guid?[] targetKey)
        {
            if(!typeof(ITargetedAssociation).IsAssignableFrom(relatedType))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(ITargetedAssociation), relatedType.GetType()));
            }

            var persistenceService = ApplicationServiceContext.Current.GetService(typeof(IRepositoryService<>).MakeGenericType(relatedType)) as IRepositoryService;
            var parm = Expression.Parameter(relatedType);
            var containsMethod = typeof(Enumerable).GetGenericMethod(nameof(Enumerable.Contains), new Type[] { typeof(Guid?) }, new Type[] { typeof(IEnumerable<Guid?>), typeof(Guid?) }) as System.Reflection.MethodInfo;

            Expression expr = Expression.Lambda(Expression.Call(null, containsMethod, Expression.Constant(targetKey), Expression.MakeMemberAccess(parm, relatedType.GetProperty(nameof(ITargetedAssociation.TargetEntityKey)) ?? relatedType.GetProperty(nameof(ActParticipation.PlayerEntityKey)))), parm);
            return persistenceService?.Find(expr) ?? new MemoryQueryResultSet(new Object[0]);
        }
        /// <summary>
        /// Query the specified object
        /// </summary>
        public IQueryResultSet<TObject> Query<TObject>(Expression<Func<TObject, bool>> query) where TObject : IdentifiedData, new()
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IRepositoryService<TObject>>();
            if (persistenceService != null)
            {
                return persistenceService.Find(query);
            }
            return new MemoryQueryResultSet<TObject>(new TObject[0]);
        }

        #endregion IEntitySourceProvider implementation
    }
}