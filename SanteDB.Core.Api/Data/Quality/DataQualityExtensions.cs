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
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Extensions;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Data quality extensions
    /// </summary>
    public static class DataQualityExtensions
    {
        // Tracer 
        private static Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityExtensions));

        // Data quality configuration service
        private static IDataQualityConfigurationProviderService m_dataQualityConfigurationService = ApplicationServiceContext.Current.GetService<IDataQualityConfigurationProviderService>();
        private static IServiceManager m_serviceManager = ApplicationServiceContext.Current.GetService<IServiceManager>();

        /// <summary>
        /// Tag the data quality issues on <paramref name="modelToTag"/> throwing an exception if <paramref name="throwOnError"/> is true
        /// </summary>
        public static IModelExtension TagDataQualityIssues<TModel>(this TModel modelToTag, bool throwOnError = true)
        {

            var ruleViolations = modelToTag.Validate().ToList();
            // Rule violations
            foreach (var rv in ruleViolations)
            {
                switch (rv.Priority)
                {
                    case DetectedIssuePriorityType.Error:
                        m_tracer.TraceInfo("DATA QUALITY ERROR ({0}) -> {1} ({2})", modelToTag, rv.Text, rv.TypeKey);
                        break;

                    case DetectedIssuePriorityType.Warning:
                        m_tracer.TraceInfo("DATA QUALITY WARNING ({0}) -> {1} ({2})", modelToTag, rv.Text, rv.TypeKey);
                        break;

                    case DetectedIssuePriorityType.Information:
                        m_tracer.TraceInfo("DATA QUALITY ISSUE ({0}) -> {1} ({2})", modelToTag, rv.Text, rv.TypeKey);
                        break;
                }
            }

            // Just in-case the rule was triggered and the caller never bothered to throw
            if (ruleViolations.Any(o => o.Priority == DetectedIssuePriorityType.Error) && throwOnError)
            {
                throw new DetectedIssueException(ruleViolations);
            }
            else if (modelToTag is IExtendable extendable)
            {
                if (ruleViolations.Any())
                {
                    return extendable.AddExtension(ExtensionTypeKeys.DataQualityExtension, typeof(DictionaryExtensionHandler), ruleViolations);
                }
            }
            return null;
        }

        /// <summary>
        /// Validate the <paramref name="data"/> for all configuration
        /// </summary>
        public static IEnumerable<DetectedIssue> Validate<TModel>(this TModel data)
        {
            foreach (var itm in m_dataQualityConfigurationService.GetRulesForType(data.GetType()))
            {
                foreach (var iss in data.Validate(itm))
                {
                    yield return iss;
                }
            }

            foreach(var dqi in m_serviceManager.GetServices().OfType<IExtendedDataQualityValidationProvider>().Where(v=>v.SupportedTypes.Any(t=>t.IsAssignableFrom(data.GetType()))))
            {
                foreach(var iss in dqi.Validate(data))
                {
                    yield return iss;
                }
            }
        }

        /// <summary>
        /// Validate for the ruleset
        /// </summary>
        public static IEnumerable<DetectedIssue> Validate<TModel>(this TModel data, DataQualityResourceConfiguration conf)
        {
            List<DetectedIssue> retVal = new List<DetectedIssue>(conf.Assertions.Count);
            List<TModel> exec = new List<TModel>() { data };

            foreach (var assert in conf.Assertions)
            {
                bool result = assert.Evaluation == AssertionEvaluationType.Any ? false : true;
                try
                {
                    foreach (var expression in assert.GetDelegates(data.GetType()))
                    {
                        var linqResult = expression(data);
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

                    if (result)
                    {
                        retVal.Add(new DetectedIssue(assert.Priority, $"{assert.Id}", assert.Text, DetectedIssueKeys.FormalConstraintIssue, data.ToString()));
                    }
                }
                catch (Exception e)
                {
                    m_tracer.TraceWarning("Error applying assertion {0} on {1} = {2} - {3}", assert.Id, data, false, e.Message);
                    retVal.Add(new DetectedIssue(assert.Priority, $"{assert.Id}", $"{assert.Text} (e: {e.Message})", DetectedIssueKeys.FormalConstraintIssue, data.ToString()));
                }
                finally
                {
                    m_tracer.TraceVerbose("Assertion {0} on {1} = {2}", assert.Id, data, result);
                }
            }

            return retVal;
        }
    }
}
