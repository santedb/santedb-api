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
using System;

namespace SanteDB.Core.Diagnostics.Performance
{
    /// <summary>
    /// Performance constants
    /// </summary>
    internal static class PerformanceConstants
    {
        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid ThreadPoolPerformanceCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A480");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid NetThreadPoolPerformanceCounter = new Guid("9E77D692-1F71-4442-BDA1-956D3DB1A480");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid ThreadPoolConcurrencyCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A481");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid ThreadPoolWorkerCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A482");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid NetIoThreads = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A483");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid NetPoolWorkerCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A484");


        /// <summary>
        /// Gets the thread pooling performance counter for queue depth
        /// </summary>
        public static readonly Guid ThreadPoolDepthCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A485");


    }
}