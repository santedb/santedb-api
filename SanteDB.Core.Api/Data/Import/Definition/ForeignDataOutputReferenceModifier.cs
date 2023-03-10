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
        private object m_inputValueSingleton;
        private readonly object m_lockObject = new object();

        /// <summary>
        /// Find collection extern
        /// </summary>
        public Guid? FindExtern(IEnumerable<IdentifiedData> inCollection, IForeignDataReader sourceRecord, object inputValue)
        {
            if(this.m_lookupExpression == null)
            {
                var parms = Enumerable.Range(0, sourceRecord.ColumnCount).Select(o => sourceRecord.GetName(o)).ToDictionary<String, String, Func<Object>>(o => o, o => () => sourceRecord[o]);
                parms.Add("input", () => this.m_inputValueSingleton);
                var expr = QueryExpressionParser.BuildLinqExpression(this.ExternalResource.Type, this.ExpressionXml.ParseQueryString(), "__instance", variables: parms, safeNullable: true, forceLoad: true, lazyExpandVariables: true);
                var inpar = Expression.Parameter(typeof(IdentifiedData));
                this.m_lookupExpression = Expression.Lambda<Func<IdentifiedData, bool>>(Expression.Invoke(expr, Expression.Convert(inpar, expr.Parameters[0].Type)), inpar).Compile();
            }

            lock (this.m_lockObject)
            {
                this.m_inputValueSingleton = inputValue;
                return inCollection.FirstOrDefault(o => this.ExternalResource.Type.IsAssignableFrom(o.GetType()) && this.m_lookupExpression(o))?.Key;
            }
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
