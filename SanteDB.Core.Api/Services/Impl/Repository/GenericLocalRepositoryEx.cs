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
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Generic nullifiable local repository
    /// </summary>
    public abstract class GenericLocalRepositoryEx<TModel> : GenericLocalRepository<TModel>, IRepositoryServiceEx<TModel>
        where TModel : IdentifiedData, IHasState
    {
        /// <summary>
        /// Create a new privacy service
        /// </summary>
        public GenericLocalRepositoryEx(IPolicyEnforcementService policyService, IDataPersistenceService<TModel> dataPersistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistenceService, privacyService)
        {
        }

        /// <summary>
        /// Nullify the specified object
        /// </summary>
        public virtual TModel Nullify(Guid id)
        {
            var target = base.Get(id); ;
            target.StatusConceptKey = StatusKeys.Nullified;
            return base.Save(target);
        }

        /// <summary>
        /// Touch the specified object
        /// </summary>
        /// <param name="id">The id of the object to be touched</param>
        /// <returns>The touched object</returns>
        public virtual void Touch(Guid id)
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<TModel>>();
            if (persistenceService is IDataPersistenceServiceEx<TModel> exPersistence)
            {
                this.DemandAlter(null);
                exPersistence.Touch(id, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }
            else
            {
                throw new NotSupportedException(ErrorMessages.NOT_SUPPORTED);
            }
        }
    }
}