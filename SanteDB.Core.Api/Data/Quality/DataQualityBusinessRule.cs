/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Represents a single data quality business rule
    /// </summary>
    public class DataQualityBusinessRule<TModel> : BaseBusinessRulesService<TModel>
        where TModel : IdentifiedData
    {
        // Configuration provider
        private IDataQualityConfigurationProviderService m_configurationProvider;

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityBusinessRule<TModel>));

        /// <summary>
        /// Creates a new data quality business rule
        /// </summary>
        public DataQualityBusinessRule(IDataQualityConfigurationProviderService dataQualityConfigurationProviderService)
        {
            this.m_tracer.TraceVerbose("Business rule service for {0} created", typeof(TModel).Name);
            this.m_configurationProvider = dataQualityConfigurationProviderService;
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
                        this.m_tracer.TraceInfo("DATA QUALITY ERROR ({0}) -> {1} ({2})", data, rv.Text, rv.TypeKey);
                        break;

                    case DetectedIssuePriorityType.Warning:
                        this.m_tracer.TraceInfo("DATA QUALITY WARNING ({0}) -> {1} ({2})", data, rv.Text, rv.TypeKey);
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

                if (extendable.Extensions.Any(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension))
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
            foreach (var kv in this.m_configurationProvider.GetRulesForType<TModel>())
            {
                retVal.AddRange(this.ValidateForRuleset(data, kv));
            }

            return base.Validate(data).Union(retVal).ToList();
        }

        /// <summary>
        /// Validate for the ruleset
        /// </summary>
        private IEnumerable<DetectedIssue> ValidateForRuleset(TModel data, DataQualityResourceConfiguration conf)
        {
            List<DetectedIssue> retVal = new List<DetectedIssue>(conf.Assertions.Count);
            List<TModel> exec = new List<TModel>() { data };

            foreach (var assert in conf.Assertions)
            {
                bool result = assert.Evaluation == AssertionEvaluationType.Any ? false : true;
                try
                {
                    foreach (var expression in assert.Expressions)
                    {
                        var linq = QueryExpressionParser.BuildLinqExpression<TModel>(NameValueCollection.ParseQueryString(expression), null, safeNullable: true, forceLoad: true);
                        var linqResult = (bool)linq.Compile().DynamicInvoke(data);
                        switch (assert.Evaluation)
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
                        retVal.Add(new DetectedIssue(assert.Priority, $"{assert.Id}", assert.Name, DetectedIssueKeys.FormalConstraintIssue));
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceWarning("Error applying assertion {0} on {1} = {2} - {3}", assert.Id, data, false, e.Message);
                    retVal.Add(new DetectedIssue(assert.Priority, $"{assert.Id}", $"{assert.Name} (e: {e.Message})", DetectedIssueKeys.FormalConstraintIssue));
                }
                finally
                {
                    this.m_tracer.TraceVerbose("Assertion {0} on {1} = {2}", assert.Id, data, result);
                }
            }

            return retVal;
        }
    }
}