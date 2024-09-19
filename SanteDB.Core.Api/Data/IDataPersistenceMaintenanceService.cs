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
namespace SanteDB.Core.Data
{
    /// <summary>
    /// A service which manages and maintains the underlying data persistence technology
    /// </summary>
    public interface IDataPersistenceMaintenanceService
    {
        /// <summary>
        /// Instructs the data connection manager to compact data
        /// </summary>
        void Compact();

        /// <summary>
        /// Copy the database to another location for backup purposes
        /// </summary>
        /// <param name="passkey">The passkey to use to encrypt the backup</param>
        /// <returns>The location where backup can be found</returns>
        string Backup(string passkey);

        /// <summary>
        /// Rekey all databases
        /// </summary>
        void RekeyDatabases();
    }
}
