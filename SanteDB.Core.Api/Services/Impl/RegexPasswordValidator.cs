/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: justin
 * Date: 2018-6-22
 */
using SanteDB.Core.Security.Services;
using System;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a regular expression password validator
    /// </summary>
    public abstract class RegexPasswordValidator : IPasswordValidatorService
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Regular Expression Password Validator";

        // Default password pattern
        public const string DefaultPasswordPattern = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$";

        // Regex for password validation
        private readonly Regex m_passwordRegex;

        /// <summary>
        /// Create regex password validator with specified expression
        /// </summary>
        public RegexPasswordValidator(String passwordMatch)
        {
            this.m_passwordRegex = new Regex(passwordMatch);
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
