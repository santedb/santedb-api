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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Model;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a repository that can validate
    /// </summary>
    public interface IValidatingRepositoryService<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {
        /// <summary>
        /// Validates the supplied data and returns a valid copy (or) throws an appropriate exception
        /// </summary>
        TModel Validate(TModel data);
    }
}