﻿/*
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
namespace SanteDB.Core.Security.Certs
{
    /// <summary>
    /// Extended key usage oids
    /// </summary>
    public static class ExtendedKeyUsageOids
    {
        /// <summary>
        /// Server authentication
        /// </summary>
        public const string ServerAuthentication = "1.3.6.1.5.5.7.3.1";
        /// <summary>
        /// Client authentication
        /// </summary>
        public const string ClientAuthentication = "1.3.6.1.5.5.7.3.2";
        /// <summary>
        /// Code signing certificates
        /// </summary>
        public const string CodeSigning = "1.3.6.1.5.5.7.3.3";
        /// <summary>
        /// All issuance policies. Applies primarily to CAs and controls the allowed usages for end-entity certs issued.
        /// </summary>
        /// <remarks>
        /// This will be opened up in a future release of .NET, see https://github.com/dotnet/runtime/issues/87270
        /// </remarks>
        public const string AllIssuancePolicy = "2.5.29.32.0";

    }
}
