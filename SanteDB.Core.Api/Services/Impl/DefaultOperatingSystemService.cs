/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using System;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Default operating system service
    /// </summary>
    public class DefaultOperatingSystemService : IOperatingSystemInfoService, IOperatingSystemPermissionService
    {
        /// <summary>
        /// The default name for the manufacturer provided by <see cref="DefaultOperatingSystemService"/>. 
        /// </summary>
        public const string MANUFACTURER_GENERIC = "Generic Manufacturer";

        /// <summary>
        /// Gets the version
        /// </summary>
        public string VersionString => Environment.OSVersion.VersionString;

        /// <summary>
        /// Operating system id
        /// </summary>
        /// <summary>
        /// Get the operating system type
        /// </summary>
        public OperatingSystemID OperatingSystem
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        return OperatingSystemID.MacOS;
                    case PlatformID.Unix:
                        return OperatingSystemID.Linux;
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                    case PlatformID.Xbox:
                        return OperatingSystemID.Win32;
                    default:
                        throw new InvalidOperationException("Invalid platform");
                }
            }
        }

        /// <summary>
        /// Gets the machine name
        /// </summary>
        public string MachineName => Environment.MachineName;

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string ManufacturerName => MANUFACTURER_GENERIC;

        /// <inheritdoc/>
        public bool HasPermission(OperatingSystemPermissionType permission) => true;

        /// <inheritdoc/>
        public bool RequestPermission(OperatingSystemPermissionType permission) => true;
    }
}
