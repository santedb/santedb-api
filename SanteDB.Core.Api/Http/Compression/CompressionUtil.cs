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
using SanteDB.Core.Http.Description;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Http.Compression
{
    /// <summary>
    /// Compression utilities
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // TODO: Design a shim for testing REST context functions
    public static class CompressionUtil
    {

        // Compression schems
        private static readonly IDictionary<String, ICompressionScheme> s_compressionSchemesByHeader;
        private static readonly IDictionary<HttpOptimizationMethod, ICompressionScheme> s_compressionSchemesByMethod;

        /// <summary>
        /// Initialize the compression scheme
        /// </summary>
        static CompressionUtil()
        {
            s_compressionSchemesByHeader = AppDomain.CurrentDomain.GetAllTypes().Where(t => typeof(ICompressionScheme).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(o => Activator.CreateInstance(o) as ICompressionScheme)
                .ToDictionary(o => o.AcceptHeaderName, o => o);
            s_compressionSchemesByMethod = AppDomain.CurrentDomain.GetAllTypes().Where(t => typeof(ICompressionScheme).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(o => Activator.CreateInstance(o) as ICompressionScheme)
                .ToDictionary(o => o.ImplementedMethod, o => o);
        }

        /// <summary>
        /// Get compression scheme
        /// </summary>
        public static ICompressionScheme GetCompressionScheme(String schemeHeader)
        {
            if (null == schemeHeader)
            {
                return null;
            }
            else if (s_compressionSchemesByHeader.TryGetValue(schemeHeader, out var handler))
            {
                return handler;
            }
            else
            {
                throw new NotSupportedException(schemeHeader);
            }
        }

        /// <summary>
        /// Get compression scheme
        /// </summary>
        public static ICompressionScheme GetCompressionScheme(HttpOptimizationMethod httpOptimizationMethod)
        {
            if (s_compressionSchemesByMethod.TryGetValue(httpOptimizationMethod, out var handler))
            {
                return handler;
            }
            else
            {
                throw new NotSupportedException(httpOptimizationMethod.ToString());
            }
        }

    }
}
