/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Defines a basic marker class for all service implementations
    /// </summary>
    /// <remarks><para>The <see cref="IServiceImplementation"/> interface is used to denote a class which implements a service. There is no
    /// rule which would preclude plugins from defining services which do not extend this base interface, however the presence of a common
    /// <see cref="ServiceName"/> property is useful for reflecting the current state of the application host context.</para></remarks>
    public interface IServiceImplementation
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        String ServiceName { get; }
    }
}