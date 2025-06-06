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
 * Date: 2024-12-12
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Quality
{

    /// <summary>
    /// Non-generic version of the exnteded data quality validation provider
    /// </summary>
    public interface IExtendedDataQualityValidationProvider
    {

        /// <summary>
        /// Gets the type of data that this validation provider can validae
        /// </summary>
        Type[] SupportedTypes { get; }

        /// <summary>
        /// Perform validation on the specified <paramref name="objectToValidate"/>
        /// </summary>
        /// <param name="objectToValidate">The object instance to be validated</param>
        /// <returns>The detected issues from this validation provider from <paramref name="objectToValidate"/></returns>
        IEnumerable<DetectedIssue> Validate(object objectToValidate);
    }

}
