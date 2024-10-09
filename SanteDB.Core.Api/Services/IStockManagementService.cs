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
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents the content of a stock container
    /// </summary>
    public interface IStockContainerContent
    {
        /// <summary>
        /// The material which is being referenced
        /// </summary>
        Guid MatierialKey { get; }

        /// <summary>
        /// Gets the material data
        /// </summary>
        ManufacturedMaterial Material { get; }

        /// <summary>
        /// The quantity of material in the container
        /// </summary>
        int Quantity { get; }
    
    }

    /// <summary>
    /// Represents a single ledger entry
    /// </summary>
    public interface IStockLedgerEntry : IIdentifiedResource
    {

        /// <summary>
        /// The sequence of the ledger entry
        /// </summary>
        long LedgerSequence { get; }

        /// <summary>
        /// The material to which the ledger entry is referring
        /// </summary>
        Guid MaterialKey { get; }

        /// <summary>
        /// Gets the reason for the ledger entry existing
        /// </summary>
        Guid ReasonKey { get; }

        /// <summary>
        /// Get the act which caused this ledger entry to be created
        /// </summary>
        Guid ActKey { get; }

        /// <summary>
        /// Get the timestamp of the ledger entry 
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the start of line balance
        /// </summary>
        int StartOfLine { get; }

        /// <summary>
        /// GEts the quantity of this ledger entry
        /// </summary>
        int Quantity { get; }

        /// <summary>
        /// Gets the end of line balance for the ledger entry
        /// </summary>
        int EndOfLine { get; }

    }


    /// <summary>
    /// Represents a stock management repository service.
    /// </summary>
    [System.ComponentModel.Description("Stock Management Provider")]
    public interface IStockManagementService : IServiceImplementation
    {

        /// <summary>
        /// Gets all stock containers for the specified <paramref name="placeKey"/>
        /// </summary>
        IQueryResultSet<Container> GetStockContainers(Guid placeKey);

        /// <summary>
        /// Get the contents of the container 
        /// </summary>
        /// <param name="containerKey">The container for which contents should be retrieved</param>
        /// <param name="dateOfBalanceReport">The option point in time for which the balance should be retrieved</param>
        /// <returns>The list of container content</returns>
        IEnumerable<IStockContainerContent> GetContainerContents(Guid containerKey, DateTimeOffset? dateOfBalanceReport = null);

        /// <summary>
        /// Get the stock ledger entries
        /// </summary>
        /// <param name="containerKey">The container for which the ledger entries are to be fetched</param>
        /// <param name="materialKey">The material for which the ledger entries are to be fetched</param>
        /// <param name="fromDate">The lower bound of dates for which the ledger entries are to be fetched</param>
        /// <param name="toDate">The upper bound of dates for which the ledger entries are to be fetched</param>
        /// <returns>The query result set of the ledger entries</returns>
        IQueryResultSet<IStockLedgerEntry> GetLedgerEntries(Guid containerKey, Guid materialKey, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null);

    }
}