﻿/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Model.DataTypes;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a repository service for managing assigning authorities.
    /// </summary>
    public interface IAssigningAuthorityRepositoryService : IRepositoryService<AssigningAuthority>
    {

        /// <summary>
        /// Get by domain
        /// </summary>
        AssigningAuthority Get(String domain);

        /// <summary>
        /// Get assigning authority by uri 
        /// </summary>
        AssigningAuthority Get(Uri uri);
    }
}