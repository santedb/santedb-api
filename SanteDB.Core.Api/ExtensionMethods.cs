/*
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
 * Date: 2021-11-5
 */
using SanteDB.Core.Interfaces;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core
{
    /// <summary>
    /// Extension methods for the core API functions
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create injected class
        /// </summary>
        public static object CreateInjected(this Type me)
        {
            return ApplicationServiceContext.Current.GetService<IServiceManager>().CreateInjected(me);
        }
    }
}