﻿using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Data.Import.Format;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

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

        /// <summary>
        /// DI constructor
        /// </summary>
        public DefaultForeignDataImporter(ILocalizationService localizationService, IServiceManager serviceManager, IThreadPoolService threadPoolService, IRepositoryService<Bundle> repositoryService)
        {
            this.m_localizationService = localizationService;
            this.m_transforms = serviceManager.CreateInjectedOfAll<IForeignDataElementTransform>().ToDictionary(o => o.Name, o => o);
            this.m_threadPool = threadPoolService;
            this.m_bundleService = repositoryService;
        }

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Import(ForeignDataObjectMap foreignDataObjectMap, IForeignDataReader sourceReader, IForeignDataWriter rejectWriter, TransactionMode transactionMode)
        {
            if (foreignDataObjectMap == null)
            {
                throw new ArgumentNullException(nameof(foreignDataObjectMap));
            }
            else if (sourceReader == null)
            {
                throw new ArgumentNullException(nameof(sourceReader));
            }
            else if (rejectWriter == null)
            {
                throw new ArgumentNullException(nameof(rejectWriter));
            }

            // Now we want to map a map of our services
            var persistenceServices = foreignDataObjectMap.Resource.ToDictionary(o => o.TypeXml, o =>
            {
                var persistenceServiceType = typeof(IDataPersistenceService<>).MakeGenericType(o.Type);
                return ApplicationServiceContext.Current.GetService(persistenceServiceType) as IDataPersistenceService;
            });
            var duplicateCheckParms = new Dictionary<String, Func<Object>>();
            for (int i = 0; i < sourceReader.ColumnCount; i++)
            {
                var parmNo = i;
                duplicateCheckParms.Add(sourceReader.GetName(i), () => sourceReader[parmNo]);
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
                        if(resourceMap.OnlyWhen?.All(s => this.CheckWhenCondition(s, sourceReader)) == false)
                        {
                            yield return new DetectedIssue(DetectedIssuePriorityType.Information, "skip", $"Only when condition indicates a skip of record {sourceReader.RowNumber}", DetectedIssueKeys.AlreadyDoneIssue);
                            continue;
                        }

                        mappedObject = null;
                        var persistenceService = persistenceServices[resourceMap.TypeXml];

                        // Is there a duplicate check? If so map them
                        var duplicateChecks = resourceMap.DuplicateCheck?.Where(o=>!o.Contains("$output")).Select(o => QueryExpressionParser.BuildLinqExpression(resourceMap.Type, o.ParseQueryString(), "o", variables: duplicateCheckParms)).ToList();
                        this.m_tracer.TraceInfo("Processing {0} from import...", records);

                        this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.5f, String.Format(UserMessages.IMPORTING, records++, 1000.0f * (float)records / (float)sw.ElapsedMilliseconds)));
                        if (duplicateChecks.Any())
                        {
                            mappedObject = duplicateChecks.Select(o => persistenceService.Query(o).FirstOrDefault())?.FirstOrDefault() as IdentifiedData;
                            mappedObject = mappedObject?.ResolveOwnedRecord(AuthenticationContext.Current.Principal);
                            if(mappedObject != null)
                            {
                                mappedObject.BatchOperation = Model.DataTypes.BatchOperationType.Update;
                            }
                        }

                        if (resourceMap.Transform != null) // Pass it to the transform
                        {
                            if (this.ApplyTransformer(resourceMap.Transform, new GenericForeignDataRecord(sourceReader), sourceReader, out var result) && result is IdentifiedData id)
                            {
                                mappedObject = id;
                            }
                            else
                            {
                                var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                                var description = this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = resourceMap.Transform.Transformer });
                                currentRecord["import_error"] = description;
                                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "txf", description, DetectedIssueKeys.OtherIssue);
                                rejectWriter.WriteRecord(new GenericForeignDataRecord(sourceReader));
                                skipProcessing = true;
                                break;
                            }
                        }
                        else if (!this.ApplyMapping(resourceMap, sourceReader, insertBundle, ref mappedObject, out var issue))
                        {
                            var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                            currentRecord["import_error"] = issue.Text;
                            yield return issue;
                            rejectWriter.WriteRecord(new GenericForeignDataRecord(sourceReader));
                            skipProcessing = true;
                            break;
                        }


                        if(mappedObject.BatchOperation != Model.DataTypes.BatchOperationType.Update &&
                            resourceMap.DuplicateCheck?.Any(o=>o.Contains("$output")) == true)
                        {
                            duplicateChecks = resourceMap.DuplicateCheck?.Where(o => o.Contains("$output")).Select(o => QueryExpressionParser.BuildLinqExpression(resourceMap.Type, o.ParseQueryString(), "o", variables: duplicateCheckParms)).ToList();
                            var shouldSkip = duplicateChecks.Any(o => persistenceService.Query(o).Any());
                            if(shouldSkip == true)
                            {
                                mappedObject.BatchOperation = Model.DataTypes.BatchOperationType.Ignore;
                            }
                        }
                        insertBundle.Add(mappedObject);
                    }

                    // Issue processing so don't process
                    if(skipProcessing)
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
                        var ce = ex;
                        var errorMessage = String.Empty;
                        while (ce != null)
                        {
                            errorMessage += ce.Message;
                            ce = ce.InnerException;
                            if (ce != null)
                            {
                                errorMessage += " CAUSE: ";
                            }
                        }
                        errorIssue = new DetectedIssue(DetectedIssuePriorityType.Error, "persistence", errorMessage, Guid.Empty);
                        this.m_tracer.TraceWarning("Could not persist import record - {0}", ex.Message);
                    }
                    if (errorIssue != null)
                    {
                        yield return errorIssue;
                        rejectWriter.WriteRecord(new GenericForeignDataRecord(sourceReader));
                    }
                }
            }
        }

        /// <summary>
        /// Check a when condition against the source reader
        /// </summary>
        private bool CheckWhenCondition(ForeignDataMapOnlyWhenCondition s, IForeignDataReader sourceReader)
        {
            return sourceReader[s.Source]?.ToString().Equals(s.Value) == true;
        }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Validate(ForeignDataObjectMap foreignDataObjectMap, IForeignDataReader sourceReader)
        {
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
                foreach (var itm in foreignDataObjectMap.Resource.SelectMany(o=>o.Maps).Where(o => !String.IsNullOrEmpty(o.Source)))
                {
                    if (sourceReader.IndexOf(itm.Source) < 0)
                    {
                        yield return new DetectedIssue(DetectedIssuePriorityType.Error, "missingField", $"Source is missing field {itm.Source}", Guid.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Apply mapping against the current row
        /// </summary>
        /// <param name="mapping">The mapping to apply</param>
        /// <param name="sourceReader">The source reader</param>
        /// <param name="mappedObject">The mapped object</param>
        /// <param name="issue">The issue which caused the result to fail</param>
        /// <param name="insertBundle">The bundle to be inserted</param>
        /// <returns>True if the mapping succeeds</returns>
        private bool ApplyMapping(ForeignDataElementResourceMap mapping, IForeignDataReader sourceReader, Bundle insertBundle, ref IdentifiedData mappedObject, out DetectedIssue issue)
        {
            try
            {
                mappedObject = mappedObject ?? Activator.CreateInstance(mapping.Type) as IdentifiedData;
                mappedObject.Key = mappedObject.Key ?? Guid.NewGuid();
                // Apply the necessary instructions
                foreach (var map in mapping.Maps)
                {

                    // Conditional mapping?
                    if (map.OnlyWhen?.All(s => this.CheckWhenCondition(s, sourceReader)) == false)
                    {
                        issue = new DetectedIssue(DetectedIssuePriorityType.Information, "skip", $"Only when condition indicates a skip of record {sourceReader.RowNumber}", DetectedIssueKeys.AlreadyDoneIssue);
                        return true;
                    }

                    if (String.IsNullOrEmpty(map.TargetHdsiPath))
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
                                        if (!this.ApplyTransformer(tx, new GenericForeignDataRecord(sourceReader), targetValue, out targetValue))
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
                                    case ForeignDataOutputReferenceModifier or:
                                        if (or.ExternalResource != null)
                                        {
                                            targetValue = or.FindExtern(insertBundle.Item);
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
                        issue = new DetectedIssue(map.TargetMissing, "maperr", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TARGET_MISSING, new { row = sourceReader.RowNumber, field = map.Source, value = sourceValue }), DetectedIssueKeys.BusinessRuleViolationIssue);
                        return false;
                    }

                    mappedObject.GetOrSetValueAtPath(map.TargetHdsiPath, targetValue, replace: map.ReplaceExisting);

                }

                issue = null;
                return true;
            }
            catch (Exception e)
            {
                issue = new DetectedIssue(DetectedIssuePriorityType.Error, "err", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_GEN_ERR, new { row = sourceReader.RowNumber, ex = e.Message }), DetectedIssueKeys.OtherIssue);
                mappedObject = null;
                return false;
            }
        }

        /// <summary>
        /// Apply the transformer <paramref name="transform"/> to <paramref name="input"/> returning the result of the map in <paramref name="output"/>
        /// </summary>
        /// <param name="transform">Transform instructions to execute</param>
        /// <param name="input">The input value to pass to the transform</param>
        /// <param name="output">The output of th etransform</param>
        /// <returns>True if the transform succeeded</returns>
        private bool ApplyTransformer(ForeignDataTransformValueModifier transform, IForeignDataRecord sourceRecord, Object input, out object output)
        {

            if (ForeignDataImportUtil.Current.TryGetElementTransformer(transform.Transformer, out var foreignDataElementTransform))
            {
                output = foreignDataElementTransform.Transform(input, sourceRecord, transform.Arguments.ToArray());
                return true;
            }
            else
            {
                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_MISSING, new { transform = transform.Transformer }));
            }

        }

    }
}
