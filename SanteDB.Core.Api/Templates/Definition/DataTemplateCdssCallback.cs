/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-12-12
 */
using Newtonsoft.Json;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.Definition
{
    /// <summary>
    /// CDSS callback information
    /// </summary>
    [XmlType(nameof(DataTemplateCdssCallback), Namespace = "http://santedb.org/model/template")]
    [XmlRoot(nameof(DataTemplateCdssCallback), Namespace = "http://santedb.org/model/template")]
    public class DataTemplateCdssCallback
    {

        private static IDecisionSupportService m_decisionSupportService;
        private static ICdssLibraryRepository m_cdssLibraryRepository;

        /// <summary>
        /// Get decision support service
        /// </summary>
        /// <returns></returns>
        private IDecisionSupportService GetDecisionSupportService()
        {
            if (m_decisionSupportService == null)
            {
                m_decisionSupportService = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();
            }
            return m_decisionSupportService;
        }

        /// <summary>
        /// Get CDSS library repository
        /// </summary>
        private ICdssLibraryRepository GetCdssLibraryRepository()
        {
            if (m_cdssLibraryRepository == null)
            {
                m_cdssLibraryRepository = ApplicationServiceContext.Current.GetService<ICdssLibraryRepository>();
            }
            return m_cdssLibraryRepository;
        }

        /// <summary>
        /// Gets or sets the subject of the CDSS execution
        /// </summary>
        [XmlElement("subject"), JsonProperty("subject")]
        public string SubjectPath { get; set; }

        /// <summary>
        /// Gets or sets the target property
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public string TargetPath { get; set; }

        /// <summary>
        /// Gets or sets the libraries to be applied
        /// </summary>
        [XmlArray("libraries"), XmlArrayItem("apply"), JsonProperty("libraries")]
        public List<String> LibrariesToApply { get; set; }

        /// <summary>
        /// Invoke the CDSS instructions
        /// </summary>
        internal void AddCdssActions(DataTemplateDefinition dataTemplateDefinition, IdentifiedData retVal)
        {
            if (String.IsNullOrWhiteSpace(this.SubjectPath))
            {
                throw new ArgumentNullException("subject");
            }
            else if (String.IsNullOrEmpty(this.TargetPath))
            {
                throw new ArgumentNullException("target");
            }

            var subjectSelection = QueryExpressionParser.BuildPropertySelector(retVal.GetType(), this.SubjectPath, true, collectionResolutionMethod: "SingleOrDefault");
            var subject = subjectSelection.Compile().DynamicInvoke(retVal) as Patient;
            if (subject == null) {
                throw new InvalidOperationException(String.Format(ErrorMessages.OBJECT_NOT_FOUND, this.SubjectPath));
            }
            subject.Key = subject.Key ?? Guid.NewGuid();

            ICdssLibrary[] librariesToApply = null;
            if(this.LibrariesToApply?.Any() == true)
            {
                librariesToApply = this.LibrariesToApply.Select(o => this.GetCdssLibraryRepository().Find(r => r.Id == o).First()).ToArray();
            }

            var carePlan = this.GetDecisionSupportService().CreateCarePlan(subject, false,
                new Dictionary<String, object>() {
                            { CdssParameterNames.EXCLUDE_ISSUES, true },
                            { CdssParameterNames.ENCOUNTER_SCOPE, dataTemplateDefinition.Mnemonic },
                            { CdssParameterNames.INCLUDE_BACKENTRY, false },
                            {CdssParameterNames.PERIOD_OF_EVENTS, DateTime.Today }
                }, librariesToApply);

            // Now we set the data in the care plan to our target
            foreach(var itm in carePlan.Relationships.Where(r=>r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent))
            {
                retVal.GetOrSetValueAtPath(this.TargetPath, itm.TargetAct, false);
            }


        }
    }
}