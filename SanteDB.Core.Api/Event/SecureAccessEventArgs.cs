﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Event
{

    /// <summary>
    /// Represents secure access 
    /// </summary>
    public abstract class SecureAccessEventArgs : EventArgs
    {

        /// <summary>
        /// Create new secure access event args
        /// </summary>
        public SecureAccessEventArgs(IPrincipal principal)
        {
            this.Principal = principal ?? AuthenticationContext.Current.Principal;
        }

        /// <summary>
        /// Gets the principal
        /// </summary>
        public IPrincipal Principal { get; }
    }
}
