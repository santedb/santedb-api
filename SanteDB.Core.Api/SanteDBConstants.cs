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

namespace SanteDB.Core
{
    /// <summary>
    /// SanteDB constants
    /// </summary>
    public static class SanteDBConstants
    {

        /// <summary>
        /// Service trace source name
        /// </summary>
        public const string ServiceTraceSourceName = "SanteDB.Core";

        /// <summary>
        /// Data source name
        /// </summary>
        public const string DataTraceSourceName = ServiceTraceSourceName + ".Data";

        /// <summary>
        /// Trace source name for queue
        /// </summary>
        public const string QueueTraceSourceName = ServiceTraceSourceName + ".Queue";

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid ThreadPoolPerformanceCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A480");

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
        public static readonly Guid ThreadPoolNonQueuedWorkerCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A483");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid ThreadPoolErrorWorkerCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A484");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid MachinePerformanceCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A485");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid ProcessorUseCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A486");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid MemoryUseCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A487");

        /// <summary>
        /// Gets the thread pooling performance counter
        /// </summary>
        public static readonly Guid DiskUseCounter = new Guid("9E77D692-1F71-4442-BDA1-056D3DB1A488");

        /// <summary>
        /// Local users will be members of this group
        /// </summary>
        public const string LocalUserGroupName = "LOCAL_USERS";
        /// <summary>
        /// Local administrators 
        /// </summary>
        public const string LocalAdminGroupName = "LOCAL_ADMINISTRATORS";
        /// <summary>
        /// Administrators will be a member of this group
        /// </summary>
        public const string AdministratorGroupName = "ADMINISTRATORS";
        /// <summary>
        /// Users will be a member of this group
        /// </summary>
        public const string UserGroupName = "USERS";
        /// <summary>
        /// Device policies are drawn from this group
        /// </summary>
        public const string DeviceGroupName = "DEVICE";
        /// <summary>
        /// Application policies are drawn from this group
        /// </summary>
        public const string ApplicationGroupName = "APPLICATIONS";
        /// <summary>
        /// Group for anonymous users
        /// </summary>
        public const string AnonymousGroupName = "ANONYMOUS";
        /// <summary>
        /// Group for system users
        /// </summary>
        public const string SystemGroupName = "SYSTEM";
        /// <summary>
        /// Clinical staff group name
        /// </summary>
        public const string ClinicalStaffGroupName = "CLINICAL_STAFF";

        /// <summary>
        /// Automatic merging tag name
        /// </summary>
        public const string AutoMatchTagName = "$sim.automerge";
    }
}