﻿using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Data.Import.Format;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// DI constructor
        /// </summary>
        public DefaultForeignDataImporter(ILocalizationService localizationService, IServiceManager serviceManager)
        {
            this.m_localizationService = localizationService;
            this.m_transforms = serviceManager.CreateInjectedOfAll<IForeignDataElementTransform>().ToDictionary(o => o.Name, o => o);
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

            IRepositoryService repositoryService = null;
            if (transactionMode == TransactionMode.Commit)
            {
                var repositoryServiceType = typeof(IRepositoryService<>).MakeGenericType(foreignDataObjectMap.Resource.Type);
                repositoryService = ApplicationServiceContext.Current.GetService(repositoryServiceType) as IRepositoryService;
            }

            int records = 0;
            while (sourceReader.MoveNext())
            {
                IdentifiedData mappedObject = null;
                if (foreignDataObjectMap.Transform != null) // Pass it to the transform
                {
                    if (this.ApplyTransformer(foreignDataObjectMap.Transform, sourceReader, out var result) && result is IdentifiedData id)
                    {
                        mappedObject = id;
                    }
                    else
                    {
                        var currentRecord = new GenericForeignDataRecord(sourceReader);
                        yield return new DetectedIssue(DetectedIssuePriorityType.Error, "txf", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = foreignDataObjectMap.Transform.Transformer }), DetectedIssueKeys.OtherIssue);
                        rejectWriter.WriteRecord(currentRecord);
                        continue;
                    }
                }
                else if (!this.ApplyMapping(foreignDataObjectMap, sourceReader, out mappedObject, out var issue))
                {
                    rejectWriter.WriteRecord(new GenericForeignDataRecord(sourceReader));
                    yield return issue;
                    continue; // skip
                }

                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.0f, String.Format(UserMessages.IMPORTING, records++)));
                repositoryService?.Save(mappedObject);

            }
        }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Validate(ForeignDataObjectMap foreignDataObjectMap, IForeignDataReader sourceReader)
        {
            if(foreignDataObjectMap == null)
            {
                throw new ArgumentNullException(nameof(foreignDataObjectMap));
            }
            else if(sourceReader == null)
            {
                throw new ArgumentNullException(nameof(sourceReader));
            }

            // Validate the map itself
            foreach(var val in foreignDataObjectMap.Validate())
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "mapIssue", val.Message, Guid.Empty);
            }

            // Validate the map against the source
            if(foreignDataObjectMap.Maps?.Any() == true)
            {
                foreach(var itm in foreignDataObjectMap.Maps)
                {
                    if(sourceReader.IndexOf(itm.Source) < 0)
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
        private bool ApplyMapping(ForeignDataObjectMap mapping, IForeignDataReader sourceReader, out IdentifiedData mappedObject, out DetectedIssue issue)
        {
            try
            {
                mappedObject = Activator.CreateInstance(mapping.Resource.Type) as IdentifiedData;
                // Apply the necessary instructions
                foreach (var map in mapping.Maps)
                {

                    if (String.IsNullOrEmpty(map.Source)) // Generated column
                    {
                        throw new NotImplementedException(); // TODO: For transforms which look up keys and stuff
                    }
                    else
                    {
                        if(String.IsNullOrEmpty(map.TargetHdsiPath))
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
                        else if(isValueNull) // no need to process
                        {
                            continue;
                        }

                        object targetValue = sourceValue;
                        foreach (var tx in map.Transforms)
                        {
                            if (!this.ApplyTransformer(tx, targetValue, out targetValue))
                            {
                                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_ERROR, new { name = tx.Transformer, row = sourceReader.RowNumber }));
                            }
                        }

                        if(targetValue == null && map.TargetMissingSpecified)
                        {
                            issue = new DetectedIssue(map.TargetMissing, "maperr", this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TARGET_MISSING, new { row = sourceReader.RowNumber, field = map.Source, value = sourceValue }), DetectedIssueKeys.BusinessRuleViolationIssue);
                            return false;
                        }

                        mappedObject.GetOrSetValueAtPath(map.TargetHdsiPath, targetValue, replace: false);

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
        private bool ApplyTransformer(ForeignDataElementTransform transform, Object input, out object output)
        {
            if(ForeignDataImportUtil.Current.TryGetElementTransformer(transform.Transformer, out var foreignDataElementTransform))
            {
                output = foreignDataElementTransform.Transform(input, transform.Arguments.ToArray());
                return true;
            }
            else
            {
                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.FOREIGN_DATA_TRANSFORM_MISSING, new { transform = transform.Transformer }));
            }
        }

    }
}
