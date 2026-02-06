/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model;

namespace SanteDB.Core.ViewModel.Null
{
    /// <summary>
    /// View model serializer which is a null view model serializer
    /// </summary>
    public interface INullTypeFormatter : IViewModelTypeFormatter
    {

        /// <summary>
        /// Serialize specified object <paramref name="o"/> into the oblivion known as null
        /// </summary>
        /// <param name="o">The object to be graphed</param>
        /// <param name="context">The current serialization context</param>
        void Serialize(IdentifiedData o, NullSerializationContext context);

    }
}
