﻿/*
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
 * Date: 2021-2-9
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A resource locking service 
    /// </summary>
    public interface IResourceCheckoutService
    {

        /// <summary>
        /// Try to get a lock on the resource for editing
        /// </summary>
        bool Checkout<T>(Guid key);

        /// <summary>
        /// Release the lock on the specified key
        /// </summary>
        bool Checkin<T>(Guid key);
    }
}