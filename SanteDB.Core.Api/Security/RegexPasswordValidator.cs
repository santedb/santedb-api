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
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a regular expression password validator
    /// </summary>
    public class RegexPasswordValidator : IPasswordValidatorService
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Regular Expression Password Validator";

        /// <summary>
        /// The default password pattern
        /// </summary>
        public const string DefaultPasswordPattern = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{4,}$";

        // Regex for password validation
        private readonly Regex m_passwordRegex;

        /// <summary>
        /// Create regex password validator with specified expression
        /// </summary>
        public RegexPasswordValidator(IConfigurationManager configurationManager)
        {
            this.m_passwordRegex = new Regex(configurationManager.GetSection<SecurityConfigurationSection>()?.PasswordRegex ?? DefaultPasswordPattern, RegexOptions.Compiled);
        }

        /// <summary>
        /// Validate the specified password
        /// </summary>
        public bool Validate(string password)
        {
            return this.m_passwordRegex.IsMatch(password);
        }

    }
}
