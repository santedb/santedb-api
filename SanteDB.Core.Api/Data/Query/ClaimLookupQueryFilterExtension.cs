/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-2-2
 */
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Query;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Core.Data.Query
{
    /// <summary>
    /// Represents a query filter extension for <c>:(claim|CLAIM_TYPE)value</c>
    /// </summary>
    public class ClaimLookupQueryFilterExtension : IQueryFilterExtension
    {
        /// <inheritdoc/>
        public string Name => "getClaim";

        /// <inheritdoc/>
        public MethodInfo ExtensionMethod => typeof(ExtensionMethods).GetMethod(nameof(ExtensionMethods.ClaimLookup));

        /// <inheritdoc/>
        public BinaryExpression Compose(Expression scope, ExpressionType comparison, Expression valueExpression, Expression[] parms)
        {
            if (comparison != ExpressionType.Equal && comparison != ExpressionType.NotEqual)
            {
                throw new InvalidOperationException();
            }
            else if (parms.Length != 1)
            {
                throw new ArgumentException(String.Format(ErrorMessages.ARGUMENT_COUNT_MISMATCH, 1, parms.Length));
            }

            // Next we want to call our method and pass that as the first parameter
            return Expression.MakeBinary(comparison,
                Expression.Call(this.ExtensionMethod, scope, parms[0]),  // Calls the extension method for getting the claim;
                valueExpression); // Pass the filter expression
        }
    }
}
