﻿/*
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
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Represents a stock management repository service.
    /// </summary>
    [ServiceProvider("Local Stock Management Repository")]
    public class LocalStockManagementRepositoryService : IStockManagementRepositoryService
    {

        //localization service
        private readonly ILocalizationService m_localizationService;

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Local Stock Management Repository";


        /// <summary>
        /// LocalStockManagementRepositoryService constructor
        /// </summary>
        /// <param name="localizationService"></param>
        public LocalStockManagementRepositoryService(ILocalizationService localizationService)
        {
            this.m_localizationService = localizationService;
        }

        /// <summary>
        /// Performs a stock adjustment for the specified facility and material.
        /// </summary>
        /// <param name="manufacturedMaterial">The manufactured material to be adjusted.</param>
        /// <param name="place">The facility for which the stock is to be adjusted.</param>
        /// <param name="quantity">The quantity to be adjusted.</param>
        /// <param name="reason">The reason for the stock to be adjusted.</param>
        /// <returns>Act.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Act Adjust(ManufacturedMaterial manufacturedMaterial, Place place, int quantity, Concept reason)
        {
            throw new NotImplementedException(this.m_localizationService.GetString("error.type.NotImplementedException"));
        }

        /// <summary>
        /// Gets the balance for the material.
        /// </summary>
        /// <param name="place">The facility for which to get the balance of stock.</param>
        /// <param name="manufacturedMaterial">The manufactured material for which to retrieve the balance.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int GetBalance(Place place, ManufacturedMaterial manufacturedMaterial)
        {
            throw new NotImplementedException(this.m_localizationService.GetString("error.type.NotImplementedException"));
        }

        /// <summary>
        /// Find adjustments
        /// </summary>
        /// <param name="manufacturedMaterialKey">The manufactured material key.</param>
        /// <param name="placeKey">The place key.</param>
        /// <param name="startPeriod">The start period.</param>
        /// <param name="endPeriod">The end period.</param>
        /// <returns>Returns a list of acts.</returns>
        /// <exception cref="System.InvalidOperationException">Unable to locate persistence service</exception>
        public IEnumerable<Act> FindAdjustments(Guid manufacturedMaterialKey, Guid placeKey, DateTimeOffset? startPeriod, DateTimeOffset? endPeriod)
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<Act>>();

            if (persistenceService == null)
            {
                throw new InvalidOperationException(this.m_localizationService.GetString("error.server.core.servicePersistence", new
                {
                    param = nameof(IDataPersistenceService<Act>)
                }));
            }

            return persistenceService.Query(o => o.ClassConceptKey == ActClassKeys.AccountManagement && o.ActTime >= startPeriod.Value && o.ActTime <= endPeriod.Value &&
                o.Participations.Where(guard => guard.ParticipationRole.Mnemonic == "Location").Any(p => p.PlayerEntityKey == placeKey) &&
                o.Participations.Where(guard => guard.ParticipationRole.Mnemonic == "Consumable").Any(p => p.PlayerEntityKey == manufacturedMaterialKey), AuthenticationContext.Current.Principal);

        }

        /// <summary>
        /// Get total consumed in the specified place by material
        /// </summary>
        public IEnumerable<ActParticipation> GetConsumed(Guid manufacturedMaterialKey, Guid placeKey, DateTimeOffset? startPeriod, DateTimeOffset? endPeriod)
        {
            var persistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<ActParticipation>>();
            if (persistenceService == null)
            {
                throw new InvalidOperationException(this.m_localizationService.GetString("error.server.core.servicePersistence", new
                {
                    param = nameof(IDataPersistenceService<ActParticipation>)
                }));
            }

            return persistenceService.Query(o => o.ParticipationRoleKey == ActParticipationKeys.Consumable && o.PlayerEntityKey == manufacturedMaterialKey && o.Act.ActTime >= startPeriod && o.Act.ActTime <= endPeriod && o.Act.Participations.Where(p => p.ParticipationRole.Mnemonic == "Location").Any(p => p.PlayerEntityKey == placeKey), AuthenticationContext.Current.Principal);
        }
    }
}