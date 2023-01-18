using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Foreign data output reference modifier to lookup on self
    /// </summary>
    [XmlType(nameof(ForeignDataOutputReferenceModifier), Namespace="http://santedb.org/import")]
    public class ForeignDataOutputReferenceModifier : ForeignDataValueModifier
    {

        // Cached lookup
        private Func<IdentifiedData, bool> m_lookupExpression;
        private Func<IdentifiedData, dynamic> m_selectExpression;

        /// <summary>
        /// Find collection extern
        /// </summary>
        public Guid? FindExtern(IEnumerable<IdentifiedData> inCollection)
        {
            if(this.m_lookupExpression == null)
            {
                var expr = QueryExpressionParser.BuildLinqExpression(this.ExternalResource.Type, this.ExpressionXml.ParseQueryString());
                var inpar = Expression.Parameter(typeof(IdentifiedData));
                this.m_lookupExpression = Expression.Lambda<Func<IdentifiedData, bool>>(Expression.Invoke(expr.Body, Expression.Convert(inpar, expr.Parameters[0].Type)), inpar).Compile();
            }
            return inCollection.FirstOrDefault(o=> this.ExternalResource.Type.IsAssignableFrom(o.GetType()) && this.m_lookupExpression(o))?.Key;
        }

        /// <summary>
        /// Select value 
        /// </summary>
        public object SelectValue(IdentifiedData currentObject)
        {
            if(this.ExternalResource != null)
            {
                throw new InvalidOperationException();
            }
            if (this.m_selectExpression == null)
            {
                var expr = QueryExpressionParser.BuildPropertySelector(currentObject.GetType(), this.ExpressionXml, true, typeof(Object));
                var inpar = Expression.Parameter(typeof(IdentifiedData));
                this.m_selectExpression = Expression.Lambda<Func<IdentifiedData, dynamic>>(Expression.Invoke(expr.Body, Expression.Convert(inpar, expr.Parameters[0].Type)), inpar).Compile();
            }
            return this.m_selectExpression(currentObject);
        }

        /// <summary>
        /// Lookup another resource in the bundle
        /// </summary>
        [XmlElement("previousEntry"), JsonProperty("previousEntry")]
        public ResourceTypeReferenceConfiguration ExternalResource { get; set; }

        /// <summary>
        /// Gets or sets the expression
        /// </summary>
        [XmlElement("expression"), JsonProperty("expression")]
        public string ExpressionXml { get; set; }

    }
}
