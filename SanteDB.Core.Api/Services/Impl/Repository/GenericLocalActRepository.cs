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
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Linq;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Represents an act repository service.
    /// </summary>
    /// <seealso cref="Services.IRepositoryService{Act}" />
    /// <seealso cref="Services.IRepositoryService{SubstanceAdministration}" />
    /// <seealso cref="Services.IRepositoryService{QuantityObservation}" />
    /// <seealso cref="Services.IRepositoryService{PatientEncounter}" />
    /// <seealso cref="Services.IRepositoryService{CodedObservation}" />
    /// <seealso cref="Services.IRepositoryService{TextObservation}" />
    public class GenericLocalActRepository<TAct> : GenericLocalRepositoryEx<TAct>, ICancelRepositoryService<TAct>
        where TAct : Act
    {
        /// <summary>
        /// DI constructor
        /// </summary>
        public GenericLocalActRepository(IPolicyEnforcementService policyService, IDataPersistenceService<TAct> dataPersistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistenceService, privacyService)
        {
        }

        /// <summary>
        /// Insert or update the specified act
        /// </summary>
        public TAct Cancel(Guid id)
        {
            var act = base.Get(id);
            act.StatusConceptKey = StatusKeys.Cancelled;
            return base.Save(act);
        }

        /// <summary>
        /// Validates an act.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The validated act data.</returns>
        /// <exception cref="DetectedIssueException">If there are any validation errors detected.</exception>
        /// <exception cref="System.InvalidOperationException">If the data is null.</exception>
        public override TAct Validate(TAct data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            base.Validate(data);

            if (data.LoadProperty(o => o.Participations).All(o => o.ParticipationRoleKey != ActParticipationKeys.Authororiginator))
            {
                var userService = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>();
                var currentUserEntity = userService.GetUserEntity(AuthenticationContext.Current.Principal.Identity);
                if (currentUserEntity != null)
                {
                    data.Participations.Add(new ActParticipation(ActParticipationKeys.Authororiginator, currentUserEntity));
                }
            }
            return data;
        }

        /// <inheritdoc/>
        public override void DemandWrite(object data) => this.DemandWritePermission(data.GetType());

        /// <inheritdoc/>
        public override void DemandAlter(object data) => this.DemandWritePermission(data.GetType());

        /// <inheritdoc/>
        public override void DemandQuery()
        {
            switch(typeof(TAct).GetResourceSensitivityClassification())
            {
                case Model.Attributes.ResourceSensitivityClassification.PersonalHealthInformation:
                    this.m_policyService.Demand(PermissionPolicyIdentifiers.QueryClinicalData);
                    break;
                case Model.Attributes.ResourceSensitivityClassification.Administrative:
                    this.m_policyService.Demand(PermissionPolicyIdentifiers.ReadAdministrativeActs);
                    break;
                default:
                    base.DemandQuery();
                    break;
            }
        }

        /// <inheritdoc/>
        public override void DemandRead(Guid key)
        {
            switch (typeof(TAct).GetResourceSensitivityClassification())
            {
                case Model.Attributes.ResourceSensitivityClassification.PersonalHealthInformation:
                    this.m_policyService.Demand(PermissionPolicyIdentifiers.ReadAdministrativeActs);
                    break;
                case Model.Attributes.ResourceSensitivityClassification.Administrative:
                    this.m_policyService.Demand(PermissionPolicyIdentifiers.ReadAdministrativeActs);
                    break;
                default:
                    base.DemandQuery();
                    break;
            }
        }

        /// <inheritdoc/>
        public override void DemandDelete(Guid key) => DemandWritePermission(typeof(TAct));


        private void DemandWritePermission(Type tAct)
        {
            switch (tAct.GetResourceSensitivityClassification())
            {
                case Model.Attributes.ResourceSensitivityClassification.PersonalHealthInformation:
                    this.m_policyService.Demand(PermissionPolicyIdentifiers.WriteClinicalData);
                    break;
                case Model.Attributes.ResourceSensitivityClassification.Administrative:
                    this.m_policyService.Demand(PermissionPolicyIdentifiers.WriteAdministrativeActs);
                    break;
                default:
                    this.m_policyService.Demand(base.WritePolicy);
                    break;
            }
        }
    }
}