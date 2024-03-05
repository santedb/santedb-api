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
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Model.Interfaces;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Taggable persistence service
    /// </summary>
    public interface ITagPersistenceService : IServiceImplementation
    {

        /// <summary>
        /// Save tag to source key
        /// </summary>
        void Save(Guid sourceKey, ITag tag);

        /// <summary>
        /// Save the tag against <paramref name="sourceKey"/>
        /// </summary>
        /// <param name="sourceKey">The source <see cref="ITaggable"/> to add <paramref name="tagName"/></param>
        /// <param name="tagName">The name of the tag</param>
        /// <param name="tagValue">The value of the tag</param>
        void Save(Guid sourceKey, String tagName, String tagValue);
    }
}
