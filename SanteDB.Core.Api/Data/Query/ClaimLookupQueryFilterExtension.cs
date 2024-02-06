using SanteDB.Core.i18n;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
            if(comparison != ExpressionType.Equal && comparison != ExpressionType.NotEqual)
            {
                throw new InvalidOperationException();
            }
            else if(parms.Length != 1)
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
