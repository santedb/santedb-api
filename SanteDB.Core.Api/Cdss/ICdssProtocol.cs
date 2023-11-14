/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents a clinical protocol
    /// </summary>
    public interface ICdssProtocol : ICdssAsset
    {

        /// <summary>
        /// The scopes (types of encounters) in which the asset belongs
        /// </summary>
        [QueryParameter("scope")]
        IEnumerable<ICdssProtocolScope> Scopes { get; }

        /// <summary>
        /// Calculate the clinical protocol for the given target data
        /// </summary>
        IEnumerable<Object> ComputeProposals(Patient target, IDictionary<String, Object> parameters);

    }
}
