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
 * Date: 2022-5-30
 */
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Simple TFA secret generator
    /// </summary>
    public class SimpleTfaSecretGenerator : ITwoFactorSecretGenerator
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Simple TFA Secret Generator";

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name
        {
            get
            {
                return "Simple TFA generator";
            }
        }

        /// <summary>
        /// Generate the TFA secret
        /// </summary>
        public string GenerateTfaSecret()
        {
            var secretInt = DateTime.Now.Ticks % 9999;
            return String.Format("{0:000000}", secretInt);
        }

        /// <summary>
        /// Validate the secret
        /// </summary>
        public bool Validate(string secret)
        {
            int toss;
            return secret.Length == 6 && Int32.TryParse(secret, out toss);
        }
    }
}
