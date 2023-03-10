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
 * Date: 2023-3-10
 */
namespace SanteDB.Core
{
    /// <summary>
    /// Operating systems
    /// </summary>
    public enum OperatingSystemID
    {
        /// <summary>
        /// Host is running on Win32
        /// </summary>
        Win32 = 0x1,
        /// <summary>
        /// Host is running on Linux
        /// </summary>
        Linux = 0x2,
        /// <summary>
        /// Host is running on MacOS
        /// </summary>
        MacOS = 0x4,
        /// <summary>
        /// Host is running on Android
        /// </summary>
        Android = 0x8,
        /// <summary>
        /// iOS Based devices
        /// </summary>
        iOS = 0x10,
        /// <summary>
        /// Host is another operating system (don't use any native libraries)
        /// </summary>
        Other = 0x20
    }

}
