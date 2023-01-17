using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Data.Import.Format;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// DI constructor
        /// </summary>
        public DefaultForeignDataImporter(ILocalizationService localizationService, IServiceManager serviceManager, IThreadPoolService threadPoolService)
        {
            this.m_localizationService = localizationService;
            this.m_transforms = serviceManager.CreateInjectedOfAll<IForeignDataElementTransform>().ToDictionary(o => o.Name, o => o);
            this.m_threadPool = threadPoolService;
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

            IDataPersistenceService persistenceService = null;
            IRepositoryService repositoryService = null;
            if (transactionMode == TransactionMode.Commit)
            {
                var persistenceServiceType = typeof(IDataPersistenceService<>).MakeGenericType(foreignDataObjectMap.Resource.Type);
                persistenceService = ApplicationServiceContext.Current.GetService(persistenceServiceType) as IDataPersistenceService;
                persistenceServiceType = typeof(IRepositoryService<>).MakeGenericType(foreignDataObjectMap.Resource.Type);
                repositoryService = ApplicationServiceContext.Current.GetService(persistenceServiceType) as IRepositoryService;

            }

            var duplicateCheckParms = new Dictionary<String, Func<Object>>(foreignDataObjectMap.DuplicateCheck.Count);
            for (int i = 0; i < sourceReader.ColumnCount; i++)
            {
                var parmNo = i;
                duplicateCheckParms.Add(sourceReader.GetName(i), () => sourceReader[parmNo]);
            }
            int records = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (sourceReader.MoveNext())
            {
                using (DataPersistenceControlContext.Create(loadMode: LoadMode.QuickLoad, autoInsert: true, autoUpdate: true))
                {
                    IdentifiedData mappedObject = null;

                    // Is there a duplicate check? If so map them
                    var duplicateChecks = foreignDataObjectMap.DuplicateCheck?.Select(o => QueryExpressionParser.BuildLinqExpression(foreignDataObjectMap.Resource.Type, o.ParseQueryString(), "o", variables: duplicateCheckParms)).ToList();

                    this.m_tracer.TraceInfo("Processing {0} from import...", records);

                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.5f, String.Format(UserMessages.IMPORTING, records++, 1000.0f * (float)records / (float)sw.ElapsedMilliseconds)));
                    if (duplicateChecks.Any())
                    {
                        mappedObject = duplicateChecks.Select(o => persistenceService.Query(o).FirstOrDefault())?.FirstOrDefault() as IdentifiedData;
                        mappedObject = mappedObject?.ResolveOwnedRecord(AuthenticationContext.Current.Principal);
                    }

                    if (foreignDataObjectMap.Transform != null) // Pass it to the transform
                    {
                        if (this.ApplyTransformer(foreignDataObjectMap.Transform, new GenericForeignDataRecord(sourceReader), sourceReader, out var result) && result is IdentifiedData id)
                        {
                            mappedObject = id;
                        }
                        else
                        {
                            var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                            var description = this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = foreignDataObjectMap.Transform.Transformer });
                            currentRecord["import_error"] = description;
                            yield return new DetectedIssue(DetectedIssuePriorityType.Error, "txf", description, DetectedIssueKeys.OtherIssue);
                            rejectWriter.WriteRecord(new GenericForeignDataRecord(sourceReader));
                            continue;
                        }
                    }
                    else if (!this.ApplyMapping(foreignDataObjectMap, sourceReader, ref mappedObject, out var issue))
                    {
                        var currentRecord = new GenericForeignDataRecord(sourceReader, "import_error");
                        currentRecord["import_error"] = issue.Text;
                        yield return issue;
                        rejectWriter.WriteRecord(new GenericForeignDataRecord(sourceReader));
                        continue;
                    }


                    DetectedIssue errorIssue = null;
                    try
                    {
                        repositoryService?.Save(mappedObject);
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
            if (foreignDataObjectMap.Maps?.Any() == true)
            {
                foreach (var itm in foreignDataObjectMap.Maps.Where(o => !String.IsNullOrEmpty(o.Source)))
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
        /// <returns>True if the mapping succeeds</returns>
        private bool ApplyMapping(ForeignDataObjectMap mapping, IForeignDataReader sourceReader, ref IdentifiedData mappedObject, out DetectedIssue issue)
        {
            try
            {
                mappedObject = mappedObject ?? Activator.CreateInstance(mapping.Resource.Type) as IdentifiedData;
                // Apply the necessary instructions
                foreach (var map in mapping.Maps)
                {

                    if (!String.IsNullOrEmpty(map.FixedValue)) // fixed value
                    {
                        mappedObject.GetOrSetValueAtPath(map.TargetHdsiPath, map.FixedValue, replace: false);
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(map.TargetHdsiPath))
                        {
                            throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_MISSING_TARGET));
                        }

                        var sourceValue = sourceReader[map.Source];
                        var isValueNull = sourceValue == null || sourceValue is String str && String.IsNullOrEmpty(str);
                        if (map.SourceRequired && isValueNull)
                        {
                            issue = new DetectedIssue(DetectedIssuePriorityType.Warning, "required", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_MAP_REQUIRED_MISSING, new { row = sourceReader.RowNumber, field = map.Source }), DetectedIssueKeys.FormalConstraintIssue);
                            return false;
                        }
                        else if (isValueNull) // no need to process
                        {
                            continue;
                        }

                        object targetValue = sourceValue;
                        foreach (var tx in map.Transforms)
                        {
                            if (!this.ApplyTransformer(tx, new GenericForeignDataRecord(sourceReader), targetValue, out targetValue))
                            {
                                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = tx.Transformer, row = sourceReader.RowNumber }));
                            }
                        }

                        if (targetValue == null && map.TargetMissingSpecified)
                        {
                            issue = new DetectedIssue(map.TargetMissing, "maperr", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TARGET_MISSING, new { row = sourceReader.RowNumber, field = map.Source, value = sourceValue }), DetectedIssueKeys.BusinessRuleViolationIssue);
                            return false;
                        }

                        mappedObject.GetOrSetValueAtPath(map.TargetHdsiPath, targetValue, replace: map.ReplaceExisting);

                    }

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
        private bool ApplyTransformer(ForeignDataElementTransform transform, IForeignDataRecord sourceRecord, Object input, out object output)
        {
            if (String.IsNullOrEmpty(transform.When) || input.ToString().Equals(transform.When))
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
            else
            {
                output = input;
            }
            return true;
        }

    }
}
