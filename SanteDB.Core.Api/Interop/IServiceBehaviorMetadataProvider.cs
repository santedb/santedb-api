/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-19
 */
using SanteDB.Core.Interop.Description;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Interop
{


    /// <summary>
    /// Represents a service behavior metadata provider
    /// </summary>
    /// <remarks>
    /// If this interface is not present then metadata interchange interfaces (WSDL, Swagger, etc.) will use reflection.
    /// </remarks>
    public interface IServiceBehaviorMetadataProvider
    {

        /// <summary>
        /// Gets the resources supported by this endpoint
        /// </summary>
        ServiceDescription Description { get; }
    }
}
