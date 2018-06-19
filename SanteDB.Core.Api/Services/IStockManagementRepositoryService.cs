﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * Date: 2017-9-1
 */
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
	/// <summary>
	/// Represents a stock management repository service.
	/// </summary>
	public interface IStockManagementRepositoryService
	{
		/// <summary>
		/// Performs a stock adjustment for the specified facility and material.
		/// </summary>
		/// <param name="manufacturedMaterial">The manufactured material to be adjusted.</param>
		/// <param name="place">The facility for which the stock is to be adjusted.</param>
		/// <param name="quantity">The quantity to be adjusted.</param>
		/// <param name="reason">The reason for the stock to be adjusted.</param>
		Act Adjust(ManufacturedMaterial manufacturedMaterial, Place place, int quantity, Concept reason);

		/// <summary>
		/// Gets the balance for the material.
		/// </summary>
		/// <param name="place">The facility for which to get the balance of stock.</param>
		/// <param name="manufacturedMaterial">The manufactured material for which to retrieve the balance.</param>
		int GetBalance(Place place, ManufacturedMaterial manufacturedMaterial);

        /// <summary>
        /// Get the total amount of consumed objects 
        /// </summary>
        /// <param name="manufacturedMaterialKey">The manufactured material being consumed</param>
        /// <param name="placeKey">The place at which the manufactured material was consumed</param>
        /// <param name="startPeriod">The start period</param>
        /// <param name="endPeriod">The end period</param>
        IEnumerable<ActParticipation> GetConsumed(Guid manufacturedMaterialKey, Guid placeKey, DateTimeOffset? startPeriod, DateTimeOffset? endPeriod);

		/// <summary>
		/// Find adjustments matching the specified
		/// </summary>
		/// <param name="manufacturedMaterialKey">The manufactured material key.</param>
		/// <param name="placeKey">The place key.</param>
		/// <param name="startPeriod">The start period.</param>
		/// <param name="endPeriod">The end period.</param>
		/// <returns>IEnumerable&lt;Act&gt;.</returns>
		IEnumerable<Act> FindAdjustments(Guid manufacturedMaterialKey, Guid placeKey, DateTimeOffset? startPeriod, DateTimeOffset? endPeriod);
	}
}