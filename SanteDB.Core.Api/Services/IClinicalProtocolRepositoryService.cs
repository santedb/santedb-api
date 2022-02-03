﻿/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using SanteDB.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Contract for service implementations which store and manage definitions of <see cref="SanteDB.Core.Model.Acts.Protocol"/> 
    /// </summary>
    /// <remarks>
    /// <para>Each protocol definition (stored in an instance of <see cref="SanteDB.Core.Model.Acts.Protocol"/>) should be backed
    /// by an implementation of the <see cref="IClinicalProtocol"/> interface. The primary responsibility of the <see cref="IClinicalProtocolRepositoryService"/>
    /// is to load these definitions from a user defined format (such as FHIR activity definitions, or the SanteDB XML CDSS format) and 
    /// generate the structured data which can be stored in the primary SanteDB database.</para>
    /// </remarks>
    [System.ComponentModel.Description("CDSS Clinical Protocol Repository")]
    public interface IClinicalProtocolRepositoryService : IServiceImplementation
    {
        /// <summary>
        /// Find protocols in the repository service
        /// </summary>
        IEnumerable<Core.Model.Acts.Protocol> FindProtocol(Expression<Func<Core.Model.Acts.Protocol, bool>> predicate, int offset, int? count, out int totalResults);

        /// <summary>
        /// Find protocols in the repository service
        /// </summary>
        Core.Model.Acts.Protocol InsertProtocol(Core.Model.Acts.Protocol data);
    }
}