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
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Data.Initialization;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Default foreign data transformer that executes the logic in <see cref="ForeignDataMap"/> files.
    /// </summary>
    public class DefaultForeignDataImporter : IForeignDataImporter, IReportProgressChanged
    {

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultForeignDataImporter));
        private readonly ILocalizationService m_localizationService;
        private readonly IDictionary<String, IForeignDataElementTransform> m_transforms;
        private readonly IThreadPoolService m_threadPool;
        private readonly IRepositoryService<Bundle> m_bundleService;
        private readonly IDatasetInstallerService m_datasetInstallService;

        /// <summary>
        /// DI constructor
        /// </summary>
        public DefaultForeignDataImporter(ILocalizationService localizationService, IServiceManager serviceManager, IThreadPoolService threadPoolService, IRepositoryService<Bundle> repositoryService, IDatasetInstallerService datasetInstaller)
        {
            this.m_localizationService = localizationService;
            this.m_transforms = serviceManager.CreateInjectedOfAll<IForeignDataElementTransform>().ToDictionary(o => o.Name, o => o);
            this.m_threadPool = threadPoolService;
            this.m_bundleService = repositoryService;
            this.m_datasetInstallService = datasetInstaller;
            if (this.m_datasetInstallService is IReportProgressChanged irpc)
            {
                irpc.ProgressChanged += (o, e) => this.ProgressChanged?.Invoke(this, e);
            }
        }

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Import(ForeignDataObjectMap foreignDataObjectMap, IDictionary<String, String> parameters, IForeignDataReader sourceReader, IForeignDataWriter rejectWriter, TransactionMode transactionMode)
        {
            if (foreignDataObjectMap == null)
            {
                throw new ArgumentNullException(nameof(foreignDataObjectMap));
            }
            else if (sourceReader == null)
            {
                throw new ArgumentNullException(nameof(sourceReader));
            }
            else if (sourceReader is IForeignDataBulkReader ifdbr)
            {
                DetectedIssue errorIssue = null;
                try
                {
                    this.m_datasetInstallService.Install(ifdbr.ReadAsDataset());
                }
                catch (Exception e)
                {
                    errorIssue = new DetectedIssue(DetectedIssuePriorityType.Error, "persistence", e.ToHumanReadableString(), Guid.Empty);
                    this.m_tracer.TraceWarning("Could not persist import record - {0}", e.Message);
                }
                if (errorIssue != null)
                {
                    yield return errorIssue;
                }
                yield break;
            }
            else if (rejectWriter == null)
            {
                throw new ArgumentNullException(nameof(rejectWriter));
            }

            // Now we want to map a map of our services
            var persistenceServices = foreignDataObjectMap.Resource.GroupBy(o => o.TypeXml).ToDictionary(o => o.Key, o =>
            {
                var persistenceServiceType = typeof(IDataPersistenceService<>).MakeGenericType(o.First().Type);
                return ApplicationServiceContext.Current.GetService(persistenceServiceType) as IDataPersistenceService;
            });
            var duplicateCheckParms = new Dictionary<String, Func<Object>>();
            for (int i = 0; i < sourceReader.ColumnCount; i++)
            {
                var parmNo = i;
                duplicateCheckParms.Add(sourceReader.GetName(i), () => sourceReader[parmNo]);
            }
            foreach (var dbck in parameters)
            {
                if (!duplicateCheckParms.ContainsKey(dbck.Key))
                {
                    duplicateCheckParms.Add(dbck.Key, () => parameters[dbck.Key]);
                }
            }

            IdentifiedData mappedObject = null;
            duplicateCheckParms.Add("output", () => mappedObject);

            int records = 0;
            var sw = new Stopwatch();
            sw.Start();
            using (DataPersistenceControlContext.Create(loadMode: LoadMode.QuickLoad, autoInsert: true, autoUpdate: true))
            {
                while (sourceReader.MoveNext())
                {

                    var skipProcessing = false;
                    var insertBundle = new Bundle();
                    foreach (var resourceMap in foreignDataObjectMap.Resource)
                    {

                        // Conditional mapping?
                        if (resourceMap.OnlyWhen?.All(s => this.CheckWhenCondition(s, sourceReader)) == false)
                        {
                            continue;
                        }

                        mappedObject = null;
                        var persistenceService = persistenceServices[resourceMap.TypeXml];

                        // Is there a duplicate check? If so map them
                        var duplicateChecks = resourceMap.DuplicateCheck?.Where(o => !o.Contains("$output")).Select(o => QueryExpressionParser.BuildLinqExpression(resourceMap.Type, o.ParseQueryString(), "o", variables: duplicateCheckParms, lazyExpandVariables: false)).ToList();

                        this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DefaultForeignDataImporter), 0.5f, String.Format(UserMessages.IMPORTING, sourceReader.RowNumber, 1000.0f * (float)sourceReader.RowNumber / (float)sw.ElapsedMilliseconds)));
                        if (duplicateChecks.Any())
                        {
                            mappedObject = duplicateChecks.Select(o => persistenceService.Query(o).FirstOrDefault())?.FirstOrDefault() as IdentifiedData;
                            mappedObject = mappedObject?.ResolveOwnedRecord(AuthenticationContext.Current.Principal);
                            if (mappedObject != null)
                            {
                                mappedObject.BatchOperation = resourceMap.PreserveExisting ? Model.DataTypes.BatchOperationType.Ignore : Model.DataTypes.BatchOperationType.Update;
                            }
                        }

                        if (resourceMap.Transform != null) // Pass it to the transform
                        {
                            if (this.ApplyTransformer(resourceMap.Transform, sourceReader, parameters, sourceReader, out var result) && result is IdentifiedData id)
                            {
                                mappedObject = id;
                            }
                            else
                            {
                                var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                                var description = this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = resourceMap.Transform.Transformer });
                                currentRecord["import_error"] = description;
                                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "txf", description, DetectedIssueKeys.OtherIssue);
                                rejectWriter.WriteRecord(currentRecord);
                                skipProcessing = true;
                                break;
                            }
                        }
                        else if (!this.ApplyMapping(resourceMap, parameters, sourceReader, insertBundle, ref mappedObject, out var issue))
                        {
                            var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                            currentRecord["import_error"] = issue.Text;
                            yield return issue;
                            rejectWriter.WriteRecord(currentRecord);
                            skipProcessing = true;
                            break;
                        }


                        if (mappedObject.BatchOperation != Model.DataTypes.BatchOperationType.Update &&
                            resourceMap.DuplicateCheck?.Any(o => o.Contains("$output")) == true)
                        {
                            duplicateChecks = resourceMap.DuplicateCheck?.Where(o => o.Contains("$output")).Select(o => QueryExpressionParser.BuildLinqExpression(resourceMap.Type, o.ParseQueryString(), "o", variables: duplicateCheckParms)).ToList();

                            var existingRecord = duplicateChecks.Select(o => persistenceService.Query(o).FirstOrDefault())?.FirstOrDefault() as IdentifiedData;
                            existingRecord = existingRecord?.ResolveOwnedRecord(AuthenticationContext.Current.Principal);
                            if (existingRecord == null)
                            {
                                existingRecord = duplicateChecks.Select(o => insertBundle.Item.Where(i => resourceMap.Type == i.GetType()).FirstOrDefault(c => o.Compile().DynamicInvoke(c).Equals(true)))?.FirstOrDefault() as IdentifiedData;
                            }
                            if (existingRecord != null)
                            {
                                mappedObject.BatchOperation = resourceMap.PreserveExisting ? Model.DataTypes.BatchOperationType.Ignore : Model.DataTypes.BatchOperationType.Update;
                                mappedObject.Key = existingRecord.Key;
                            }
                        }
                        insertBundle.Add(mappedObject);
                    }

                    // Issue processing so don't process
                    if (skipProcessing)
                    {
                        continue;
                    }

                    DetectedIssue errorIssue = null;
                    try
                    {
                        this.m_bundleService?.Save(insertBundle);
                    }
                    catch (DetectedIssueException ex)
                    {
                        errorIssue = ex.Issues.First();
                    }
                    catch (Exception ex)
                    {
                        errorIssue = new DetectedIssue(DetectedIssuePriorityType.Error, "persistence", ex.ToHumanReadableString(), Guid.Empty);
                        this.m_tracer.TraceWarning("Could not persist import record - {0}", ex);
                    }
                    if (errorIssue != null)
                    {
                        yield return errorIssue;
                        var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                        currentRecord["import_error"] = errorIssue.Text;
                        rejectWriter.WriteRecord(currentRecord);
                    }
                }
            }

        }

        /// <summary>
        /// Check a when condition against the source reader
        /// </summary>
        private bool CheckWhenCondition(ForeignDataMapOnlyWhenCondition s, IForeignDataReader sourceReader)
        {
            if (s.Value.Contains("*"))
            {
                return sourceReader[s.Source] != null;
            }
            else if(!String.IsNullOrEmpty(s.Other))
            {
                var equal = sourceReader[s.Source] == sourceReader[s.Other];
                return s.Negation ? !equal : equal;
            }
            else
            {
                return s.Value?.Contains(sourceReader[s.Source]?.ToString()) == !s.Negation;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Validate(ForeignDataObjectMap foreignDataObjectMap, IDictionary<string, string> parameters, IForeignDataReader sourceReader)
        {
            if (!sourceReader.MoveNext())
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "empty", "File appears to be empty", Guid.Empty);
            }
            if (foreignDataObjectMap == null)
            {
                throw new ArgumentNullException(nameof(foreignDataObjectMap));
            }
            else if (sourceReader == null)
            {
                throw new ArgumentNullException(nameof(sourceReader));
            }

            // Validate the map itself
            foreach (var val in foreignDataObjectMap.Validate())
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "mapIssue", val.Message, Guid.Empty);
            }

            // Validate the map against the source
            if (foreignDataObjectMap.Resource?.Any() == true)
            {
                var duplicateCheckParms = new Dictionary<String, Func<Object>>();
                for (int i = 0; i < sourceReader.ColumnCount; i++)
                {
                    var parmNo = i;
                    duplicateCheckParms.Add(sourceReader.GetName(i), () => sourceReader[parmNo]);
                }
                foreach (var dbck in parameters)
                {
                    if (!duplicateCheckParms.ContainsKey(dbck.Key))
                    {
                        duplicateCheckParms.Add(dbck.Key, () => parameters[dbck.Key]);
                    }
                }
                IdentifiedData outputFake = null;
                duplicateCheckParms.Add("output", () => outputFake);

                foreach (var res in foreignDataObjectMap.Resource.Where(o => !o.Type.IsAbstract))
                {
                    outputFake = Activator.CreateInstance(res.Type) as IdentifiedData;
                    foreach (var itm in res.Maps.Where(o => !String.IsNullOrEmpty(o.Source)))
                    {
                        if (sourceReader.IndexOf(itm.Source) < 0)
                        {
                            yield return new DetectedIssue(DetectedIssuePriorityType.Error, "missingField", $"Source is missing field {itm.Source}", Guid.Empty);
                        }
                    }

                    foreach (var wh in res.OnlyWhen)
                    {
                        if (sourceReader.IndexOf(wh.Source) < 0)
                        {
                            yield return new DetectedIssue(DetectedIssuePriorityType.Error, "missingField", $"Source is missing field {wh.Source}", Guid.Empty);
                        }
                    }

                    foreach (var dc in res.DuplicateCheck)
                    {
                        DetectedIssue issue = null;
                        try
                        {
                            QueryExpressionParser.BuildLinqExpression(res.Type, dc.ParseQueryString(), "p", variables: duplicateCheckParms);
                        }
                        catch (Exception e)
                        {
                            issue = new DetectedIssue(DetectedIssuePriorityType.Error, "checkExpression", $"Could not process duplicate check on {res.TypeXml} - {e.ToHumanReadableString()}", Guid.Empty);
                        }
                        if (issue != null)
                        {
                            yield return issue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply mapping against the current row
        /// </summary>
        /// <param name="mapping">The mapping to apply</param>
        /// <param name="parameters">Data Map parameters.</param>
        /// <param name="sourceReader">The source reader</param>
        /// <param name="mappedObject">The mapped object</param>
        /// <param name="issue">The issue which caused the result to fail</param>
        /// <param name="insertBundle">The bundle to be inserted</param>
        /// <returns>True if the mapping succeeds</returns>
        private bool ApplyMapping(ForeignDataElementResourceMap mapping, IDictionary<String, String> parameters, IForeignDataReader sourceReader, Bundle insertBundle, ref IdentifiedData mappedObject, out DetectedIssue issue)
        {
            try
            {
                mappedObject = mappedObject ?? mapping.Skeleton?.DeepCopy() as IdentifiedData ?? Activator.CreateInstance(mapping.Type) as IdentifiedData;
                mappedObject.Key = mappedObject.Key ?? Guid.NewGuid();
                // Apply the necessary instructions
                foreach (var map in mapping.Maps)
                {
                    try
                    {
                        // Conditional mapping?
                        if (map.OnlyWhen?.All(s => this.CheckWhenCondition(s, sourceReader)) == false)
                        {
                            continue;
                        }

                        if (String.IsNullOrEmpty(map.TargetHdsiPath?.Value))
                        {
                            throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_MISSING_TARGET));
                        }

                        object sourceValue = null;
                        if (!String.IsNullOrEmpty(map.Source))
                        {
                            sourceValue = sourceReader[map.Source];
                        }

                        var isValueNull = sourceValue == null || sourceValue is String str && String.IsNullOrEmpty(str);
                        if (map.SourceRequired && isValueNull)
                        {
                            issue = new DetectedIssue(DetectedIssuePriorityType.Warning, "required", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_MAP_REQUIRED_MISSING, new { row = sourceReader.RowNumber, field = map.Source }), DetectedIssueKeys.FormalConstraintIssue);
                            return false;
                        }
                        else if (isValueNull && !String.IsNullOrEmpty(map.Source)) // no need to process
                        {
                            continue;
                        }

                        object targetValue = sourceValue;

                        // What are the mappers
                        if (map.ValueModifiers != null)
                        {
                            foreach (var modifier in map.ValueModifiers)
                            {
                                if (String.IsNullOrEmpty(modifier.When) || sourceValue.ToString().Equals(modifier.When))
                                {
                                    switch (modifier)
                                    {
                                        case ForeignDataTransformValueModifier tx:
                                            if (!this.ApplyTransformer(tx, sourceReader, parameters, targetValue, out targetValue))
                                            {
                                                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = tx.Transformer, row = sourceReader.RowNumber }));
                                            }
                                            break;
                                        case ForeignDataFixedValueModifier fx:
                                            targetValue = fx.FixedValue;
                                            break;
                                        case ForeignDataLookupValueModifier lx:
                                            targetValue = sourceReader[lx.SourceColumn];
                                            break;
                                        case ForeignDataParameterValueModifier px:
                                            if (!parameters.TryGetValue(px.ParameterName, out var value))
                                            {
                                                throw new MissingFieldException(px.ParameterName);
                                            }
                                            targetValue = value;
                                            break;
                                        case ForeignDataOutputReferenceModifier or:
                                            if (or.ExternalResource != null)
                                            {
                                                targetValue = or.FindExtern(insertBundle.Item, sourceReader, targetValue);
                                            }
                                            else
                                            {
                                                targetValue = or.SelectValue(mappedObject);
                                            }
                                            break;
                                    }
                                }
                            }
                        }


                        if (targetValue == null && map.TargetMissingSpecified)
                        {
                            issue = new DetectedIssue(map.TargetMissing, "maperr", map.ErrorMessage ?? this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TARGET_MISSING, new { row = sourceReader.RowNumber, field = map.Source, value = sourceValue, target = map.TargetHdsiPath }), DetectedIssueKeys.BusinessRuleViolationIssue);
                            if (map.TargetMissing != DetectedIssuePriorityType.Information)
                            {
                                return false;
                            }
                        }

                        if (map.TargetHdsiPath.PreserveExisting)
                        {
                            var currentValue = mappedObject.GetOrSetValueAtPath(map.TargetHdsiPath.Value);
                            if (currentValue != null) continue;
                        }
                        mappedObject.GetOrSetValueAtPath(map.TargetHdsiPath.Value, targetValue, replace: map.ReplaceExisting);

                    }
                    catch(Exception e)
                    {
                        issue = new DetectedIssue(DetectedIssuePriorityType.Error, "err", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_FLD_ERR, new { field = map.TargetHdsiPath.Value, row = sourceReader.RowNumber, ex = e.ToHumanReadableString() }), DetectedIssueKeys.OtherIssue);
                        mappedObject = null;
                        return false;
                    }
                }

                issue = null;
                return true;
            }
            catch (Exception e)
            {
                issue = new DetectedIssue(DetectedIssuePriorityType.Error, "err", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_GEN_ERR, new { row = sourceReader.RowNumber, ex = e.ToHumanReadableString() }), DetectedIssueKeys.OtherIssue);
                mappedObject = null;
                return false;
            }
        }

        /// <summary>
        /// Apply the transformer <paramref name="transform"/> to <paramref name="input"/> returning the result of the map in <paramref name="output"/>
        /// </summary>
        /// <param name="transform">Transform instructions to execute</param>
        /// <param name="sourceRecord">The source record to apply the transform to</param>
        /// <param name="dataMapParameters">The data map parameters.</param>
        /// <param name="input">The input value to pass to the transform</param>
        /// <param name="output">The output of th etransform</param>
        /// <returns>True if the transform succeeded</returns>
        private bool ApplyTransformer(ForeignDataTransformValueModifier transform, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, Object input, out object output)
        {

            if (ForeignDataImportUtil.Current.TryGetElementTransformer(transform.Transformer, out var foreignDataElementTransform))
            {
                output = foreignDataElementTransform.Transform(input, sourceRecord, dataMapParameters, transform.Arguments.ToArray());
                return true;
            }
            else
            {
                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_MISSING, new { transform = transform.Transformer }));
            }

        }

    }
}
