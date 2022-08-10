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
 * Date: 2021-8-27
 */
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Interface which provides localization functions
    /// </summary>
    [System.ComponentModel.Description("Localization Provider")]
    public interface ILocalizationService : IServiceImplementation
    {
        /// <summary>
        /// Get the specified string in the current locale
        /// </summary>
        String GetString(String stringKey);

        /// <summary>
        /// Get the specified <paramref name="stringKey"/> in <paramref name="locale"/>
        /// </summary>
        String GetString(String locale, String stringKey);

        /// <summary>
        /// Format a <paramref name="stringKey"/> with <paramref name="parameters"/>
        /// </summary>
        String GetString(String stringKey, dynamic parameters);

        /// <summary>
        /// Format a <paramref name="stringKey"/> from <paramref name="locale"/> with <paramref name="parameters"/>
        /// </summary>
        String GetString(String locale, String stringKey, dynamic parameters);

        /// <summary>
        /// Get all strings in the specified locale
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<String, String>> GetStrings(String locale);

        /// <summary>
        /// Reload string definitions
        /// </summary>
        void Reload();
    }
}