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
using SanteDB.Core.i18n;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// A <see cref="IForeignDataElementTransform"/> implementation which can look up a concept based on its reference terminology
    /// </summary>
    public class ReferenceTermLookup : IForeignDataElementTransform
    {
        private readonly IConceptRepositoryService m_conceptRepositoryService;

        /// <summary>
        /// Lookup transform DI constructor
        /// </summary>
        public ReferenceTermLookup(IConceptRepositoryService concept)
        {
            this.m_conceptRepositoryService = concept;
        }

        /// <summary>
        /// Get the name of the transform
        /// </summary>
        public string Name => "ReferenceTermLookup";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            if(args.Length != 1)
            {
                throw new ArgumentException("arg1", ErrorMessages.ARGUMENT_NULL);
            }
            return this.m_conceptRepositoryService.FindConceptsByReferenceTerm(input.ToString(), args[0].ToString()).Select(o=>o.SourceEntityKey).FirstOrDefault();
        }
    }
}
