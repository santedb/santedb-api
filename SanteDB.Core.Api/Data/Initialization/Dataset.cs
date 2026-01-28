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
 * Date: 2023-6-21
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// A class representing data operations
    /// </summary>
    [XmlRoot("dataset", Namespace = "http://santedb.org/data")]
    [XmlType(nameof(Dataset), Namespace = "http://santedb.org/data")]
    public class Dataset
    {
        // Serializer
        private static XmlSerializer m_xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(Dataset));

        /// <summary>
        /// Default ctor
        /// </summary>
        public Dataset()
        {
            this.Action = new List<DataInstallAction>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dataset"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Dataset(string id) : this()
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets the identifier of the dataset
        /// </summary>
        [XmlAttribute("id")]
        public String Id { get; set; }

        /// <summary>
        /// Actions to be performed
        /// </summary>
        [XmlElement("insert", Type = typeof(DataInsert))]
        [XmlElement("obsolete", Type = typeof(DataDelete))]
        [XmlElement("update", Type = typeof(DataUpdate))]
        public List<DataInstallAction> Action { get; set; }

        /// <summary>
        /// Actions to be executed after 
        /// </summary>
        [XmlArray("sql")]
        [XmlArrayItem("exec")]
        public List<DataExecuteAction> SqlExec { get; set; }

        /// <summary>
        /// Execute SQL before 
        /// </summary>
        [XmlArray("prepare")]
        [XmlArrayItem("exec")]
        public List<DataExecuteAction> PreSqlExec { get; set; }

        /// <summary>
        /// Execute a service action on install
        /// </summary>
        [XmlArray("exec")]
        [XmlArrayItem("service")]
        public List<ServiceExecuteAction> ServiceExec { get; set; }

        /// <summary>
        /// Loads the specified file to dataset
        /// </summary>
        public static Dataset Load(Stream str)
        {
            return m_xsz.Deserialize(str) as Dataset;
        }

        /// <summary>
        /// Save the dataset to the specified stream
        /// </summary>
        public void Save(Stream str)
        {
            using (SerializationControlContext.EnterExportContext())
            {
                m_xsz.Serialize(str, this);
            }
        }
    }

    /// <summary>
    /// Service execution actions
    /// </summary>
    [XmlType(nameof(ServiceExecuteAction), Namespace = "http://santedb.org/data")]
    public class ServiceExecuteAction
    {

        /// <summary>
        /// Gets or sets the service which should be executed
        /// </summary>
        [XmlAttribute("type")]
        public String ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed
        /// </summary>
        [XmlAttribute("method")]
        public String Method { get; set; }

        /// <summary>
        /// Gets the arguments
        /// </summary>
        [XmlArray("args"), XmlArrayItem("string", typeof(String)), XmlArrayItem("int", typeof(Int32)), XmlArrayItem("bool", typeof(Boolean))]
        public List<Object> Arguments { get; set; }
    }

    /// <summary>
    /// Data execution actions
    /// </summary>
    [XmlType(nameof(DataExecuteAction), Namespace = "http://santedb.org/data")]
    public class DataExecuteAction
    {

        /// <summary>
        /// Gets or sets the invariant
        /// </summary>
        [XmlAttribute("invariant")]
        public string InvariantName { get; set; }

        /// <summary>
        /// Gets or sets the query text
        /// </summary>
        [XmlText]
        public String QueryText { get; set; }
    }

    /// <summary>
    /// Asset data action base
    /// </summary>
    [XmlType(nameof(DataInstallAction), Namespace = "http://santedb.org/data")]
    public abstract class DataInstallAction
    {

        /// <summary>
        /// Gets the action name
        /// </summary>
        public abstract String ActionName { get; }

        /// <summary>
        /// Gets the elements to be performed
        /// </summary>
        [XmlElement("ConceptReferenceTerm", typeof(ConceptReferenceTerm), Namespace = "http://santedb.org/model")]
        [XmlElement("ConceptName", typeof(ConceptName), Namespace = "http://santedb.org/model")]
        [XmlElement("EntityRelationship", typeof(EntityRelationship), Namespace = "http://santedb.org/model")]
        [XmlElement("Concept", typeof(Concept), Namespace = "http://santedb.org/model")]
        [XmlElement("ConceptSet", typeof(ConceptSet), Namespace = "http://santedb.org/model")]
        [XmlElement("ConceptSetComposition", typeof(ConceptSetComposition), Namespace = "http://santedb.org/model")]
        [XmlElement("ConceptRelationship", typeof(ConceptRelationship), Namespace = "http://santedb.org/model")]
        [XmlElement("AssigningAuthority", typeof(AssigningAuthority), Namespace = "http://santedb.org/model")]
        [XmlElement("IdentityDomain", typeof(IdentityDomain), Namespace = "http://santedb.org/model")]
        [XmlElement("ConceptClass", typeof(ConceptClass), Namespace = "http://santedb.org/model")]
        [XmlElement("SecurityPolicy", typeof(SecurityPolicy), Namespace = "http://santedb.org/model")]
        [XmlElement("SecurityRole", typeof(SecurityRole), Namespace = "http://santedb.org/model")]
        [XmlElement("SecurityApplication", typeof(SecurityApplication), Namespace = "http://santedb.org/model")]
        [XmlElement("ApplicationEntity", typeof(ApplicationEntity), Namespace = "http://santedb.org/model")]
        [XmlElement("SecurityUser", typeof(SecurityUser), Namespace = "http://santedb.org/model")]
        [XmlElement("ExtensionType", typeof(ExtensionType), Namespace = "http://santedb.org/model")]
        [XmlElement("CodeSystem", typeof(CodeSystem), Namespace = "http://santedb.org/model")]
        [XmlElement("ReferenceTerm", typeof(ReferenceTerm), Namespace = "http://santedb.org/model")]
        [XmlElement("UserEntity", typeof(UserEntity), Namespace = "http://santedb.org/model")]
        [XmlElement("Container", typeof(Container), Namespace = "http://santedb.org/model")]
        [XmlElement("Entity", typeof(Entity), Namespace = "http://santedb.org/model")]
        [XmlElement("Organization", typeof(Organization), Namespace = "http://santedb.org/model")]
        [XmlElement("Person", typeof(Person), Namespace = "http://santedb.org/model")]
        [XmlElement("Provider", typeof(Provider), Namespace = "http://santedb.org/model")]
        [XmlElement("Material", typeof(Material), Namespace = "http://santedb.org/model")]
        [XmlElement("ManufacturedMaterial", typeof(ManufacturedMaterial), Namespace = "http://santedb.org/model")]
        [XmlElement("Patient", typeof(Patient), Namespace = "http://santedb.org/model")]
        [XmlElement("Place", typeof(Place), Namespace = "http://santedb.org/model")]
        [XmlElement("Bundle", typeof(Bundle), Namespace = "http://santedb.org/model")]
        [XmlElement("Act", typeof(Act), Namespace = "http://santedb.org/model")]
        [XmlElement("SubstanceAdministration", typeof(SubstanceAdministration), Namespace = "http://santedb.org/model")]
        [XmlElement("QuantityObservation", typeof(QuantityObservation), Namespace = "http://santedb.org/model")]
        [XmlElement("CodedObservation", typeof(CodedObservation), Namespace = "http://santedb.org/model")]
        [XmlElement("EntityIdentifier", typeof(EntityIdentifier), Namespace = "http://santedb.org/model")]
        [XmlElement("TextObservation", typeof(TextObservation), Namespace = "http://santedb.org/model")]
        [XmlElement("PatientEncounter", typeof(PatientEncounter), Namespace = "http://santedb.org/model")]
        [XmlElement("RelationshipValidationRule", typeof(RelationshipValidationRule), Namespace = "http://santedb.org/model")]
        public IdentifiedData Element { get; set; }

        /// <summary>
        /// Associate the specified data for stuff that cannot be serialized
        /// </summary>
        [XmlElement("associate")]
        public List<DataAssociation> Association { get; set; }

        /// <summary>
        /// Skip if errored
        /// </summary>
        [XmlAttribute("skipIfError")]
        public bool IgnoreErrors { get; set; }

    }

    /// <summary>
    /// Associate data
    /// </summary>
    [XmlType(nameof(DataAssociation), Namespace = "http://santedb.org/data")]
    public class DataAssociation : DataInstallAction
    {

        /// <summary>
        /// Action to be performed
        /// </summary>
        public override string ActionName
        {
            get
            {
                return "Add";
            }
        }


        /// <summary>
        /// The name of the property
        /// </summary>
        [XmlAttribute("property")]
        public String PropertyName { get; set; }

    }

    /// <summary>
    /// Asset data update
    /// </summary>
    [XmlType(nameof(DataUpdate), Namespace = "http://santedb.org/data")]
    public class DataUpdate : DataInstallAction
    {
        /// <summary>
        /// Gets the action name
        /// </summary>
        public override string ActionName { get { return "Update"; } }

        /// <summary>
        /// Insert if not exists
        /// </summary>
        [XmlAttribute("insertIfNotExists")]
        public bool InsertIfNotExists { get; set; }


    }

    /// <summary>
    /// Obsoletes the specified data elements
    /// </summary>
    [XmlType(nameof(DataDelete), Namespace = "http://santedb.org/data")]
    public class DataDelete : DataInstallAction
    {
        /// <summary>
        /// Gets the action name
        /// </summary>
        public override string ActionName { get { return "Obsolete"; } }

    }

    /// <summary>
    /// Data insert
    /// </summary>
    [XmlType(nameof(DataInsert), Namespace = "http://santedb.org/data")]
    public class DataInsert : DataInstallAction
    {
        /// <summary>
        /// Gets the action name
        /// </summary>
        public override string ActionName { get { return "Insert"; } }

        /// <summary>
        /// True if the insert should be skipped if it exists
        /// </summary>
        [XmlAttribute("skipIfExists")]
        public bool SkipIfExists { get; set; }
    }
}