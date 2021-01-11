/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2020-2-28
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Extensions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Data.Quality
{



    /// <summary>
    /// Represents a single data quality business rule
    /// </summary>
    public class DataQualityBusinessRule<TModel> : BaseBusinessRulesService<TModel>, IDataQualityBusinessRuleService
        where TModel : IdentifiedData
    {

        // Remove the resource configuration
        private Dictionary<String, List<DataQualityResourceConfiguration>> m_resourceConfigurations = new Dictionary<String, List<DataQualityResourceConfiguration>>();

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityBusinessRule<TModel>));

        /// <summary>
        /// Creates a new data quality business rule
        /// </summary>
        public DataQualityBusinessRule()
        {
            this.m_tracer.TraceVerbose("Business rule service for {0} created", typeof(TModel).Name);
        }

        /// <summary>
        /// Before inserting, validation DQ
        /// </summary>
        public override TModel BeforeInsert(TModel data)
        {
            this.TagResource(data);
            return base.BeforeInsert(data);
        }

        /// <summary>
        /// Before update, validate DQ
        /// </summary>
        public override TModel BeforeUpdate(TModel data)
        {
            this.TagResource(data);
            return base.BeforeUpdate(data);
        }

        /// <summary>
        /// Adds the specified configuration to the BR
        /// </summary>
        public void AddDataQualityResourceConfiguration(String ruleSetId, DataQualityResourceConfiguration configuration)
        {
            // Validate the configuration
            foreach(var assert in configuration.Assertions)
                foreach(var expression in assert.Expressions)
                {
                    this.m_tracer.TraceVerbose("Validing expression {0}", expression);
                    QueryExpressionParser.BuildLinqExpression<TModel>(NameValueCollection.ParseQueryString(expression));
                }
            if (!this.m_resourceConfigurations.TryGetValue(ruleSetId, out List<DataQualityResourceConfiguration> current))
                this.m_resourceConfigurations.Add(ruleSetId, new List<DataQualityResourceConfiguration>() { configuration });
            else
                current.Add(configuration);
        }

        /// <summary>
        /// Tag the resource or throw an exception based on DQ rules that fail/pass
        /// </summary>
        /// <param name="data">The data to be validated</param>
        private void TagResource(TModel data)
        {
            var ruleViolations = this.Validate(data);

            // Rule violations
            foreach (var rv in ruleViolations)
            {
                switch (rv.Priority)
                {
                    case DetectedIssuePriorityType.Error:
                        this.m_tracer.TraceError("DATA QUALITY ERROR ({0}) -> {1} ({2})", data, rv.Text, rv.TypeKey);
                        break;
                    case DetectedIssuePriorityType.Warning:
                        this.m_tracer.TraceWarning("DATA QUALITY WARNING ({0}) -> {1} ({2})", data, rv.Text, rv.TypeKey);
                        break;
                    case DetectedIssuePriorityType.Information:
                        this.m_tracer.TraceInfo("DATA QUALITY ISSUE ({0}) -> {1} ({2})", data, rv.Text, rv.TypeKey);
                        break;
                }
            }

            // Just in-case the rule was triggered and the caller never bothered to throw
            if (ruleViolations.Any(o => o.Priority == DetectedIssuePriorityType.Error))
                throw new DetectedIssueException(ruleViolations);
            else if (data is IExtendable extendable)
            {
                this.m_tracer.TraceWarning("Object {0} contains {1} data quality issues", data, ruleViolations.Count);

                if(extendable.Extensions.Any(o=>o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension))
                    extendable.RemoveExtension(ExtensionTypeKeys.DataQualityExtension);
                extendable.AddExtension(ExtensionTypeKeys.DataQualityExtension, typeof(DictionaryExtensionHandler), ruleViolations);
            }

        }

        /// <summary>
        /// Validate the specified resource
        /// </summary>
        public override List<DetectedIssue> Validate(TModel data)
        {

            List<DetectedIssue> retVal = new List<DetectedIssue>();
            // Run each of the rules
            foreach (var kv in this.m_resourceConfigurations)
            {
                this.m_tracer.TraceVerbose("Validating {0} against ruleset {1}", data, kv.Key);
                foreach(var conf in kv.Value)
                    retVal.AddRange(this.ValidateForRuleset(data, kv.Key, conf));
            }

            return base.Validate(data).Union(retVal).ToList();
        }

        /// <summary>
        /// Validate for the ruleset
        /// </summary>
        private IEnumerable<DetectedIssue> ValidateForRuleset(TModel data, String ruleSetId, DataQualityResourceConfiguration conf)
        {
            List<DetectedIssue> retVal = new List<DetectedIssue>(conf.Assertions.Count);
            List<TModel> exec = new List<TModel>() { data };
            
            foreach(var assert in conf.Assertions)
            {
                bool result = assert.Evaluation == AssertionEvaluationType.Any ? false : true;
                try
                {
                    foreach(var expression in assert.Expressions)
                    {
                        var linq = QueryExpressionParser.BuildLinqExpression<TModel>(NameValueCollection.ParseQueryString(expression), null, safeNullable: true, forceLoad: true);
                        var linqResult = (bool)linq.Compile().DynamicInvoke(data);
                        switch(assert.Evaluation)
                        {
                            case AssertionEvaluationType.All:
                                result &= linqResult;
                                break;
                            case AssertionEvaluationType.Any:
                                result |= linqResult;
                                break;
                            case AssertionEvaluationType.None:
                                result &= !linqResult;
                                break;
                        }
                    }

                    if (!result)
                        retVal.Add(new DetectedIssue(assert.Priority, $"{ruleSetId}.{assert.Id}", assert.Name, DetectedIssueKeys.FormalConstraintIssue));
                }
                catch(Exception e)
                {
                    this.m_tracer.TraceWarning("Error applying assertion {0}.{1} on {2} = {3} - {4}", ruleSetId, assert.Id, data, false, e.Message);
                    retVal.Add(new DetectedIssue(assert.Priority, $"{ruleSetId}.{assert.Id}", $"{assert.Name} (e: {e.Message})", DetectedIssueKeys.FormalConstraintIssue));
                }
                finally
                {
                    this.m_tracer.TraceVerbose("Assertion {0}.{1} on {2} = {3}", ruleSetId, assert.Id, data, result);
                }
            }

            return retVal;
        }
    }
}
