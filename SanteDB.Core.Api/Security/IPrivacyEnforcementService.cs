﻿/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Privacy enforcement service
    /// </summary>
    [System.ComponentModel.Description("Data Privacy Provider")]
    public interface IPrivacyEnforcementService : IServiceImplementation
    {

        /// <summary>
        /// Apply all privacy policies
        /// </summary>
        TData Apply<TData>(TData data, IPrincipal principal) where TData : IdentifiedData;

        /// <summary>
        /// Apply the 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        IEnumerable<TData> Apply<TData>(IEnumerable<TData> data, IPrincipal principal) where TData : IdentifiedData;

        /// <summary>
        /// Determine if the record provided contains data that the <paramref name="principal"/>
        /// shouldn't be sending such as masked identifiers or the record itself (due to access permission)
        /// </summary>
        bool ValidateWrite<TData>(TData data, IPrincipal principal) where TData : IdentifiedData;
    }
}
