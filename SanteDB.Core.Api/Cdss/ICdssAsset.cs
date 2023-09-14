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
 * Date: 2023-5-19
 */
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Cdss
{

    /// <summary>
    /// Asset classification
    /// </summary>
    public enum CdssAssetClassification
    {
        /// <summary>
        /// The protocol asset is a clinical protocol which generates a care plan
        /// </summary>
        DecisionSupportProtocol = 1,
        /// <summary>
        /// The protocol asset is a library which is referenced by others
        /// </summary>
        DecisionSupportLibrary = 2
    }

    /// <summary>
    /// Represents an asset grouping
    /// </summary>
    public interface ICdssAssetGroup
    {
        /// <summary>
        /// Gets the ID of the asset group
        /// </summary>
        [QueryParameter("uuid")]
        Guid Uuid { get; }

        /// <summary>
        /// Gets the name of the asset group
        /// </summary>
        [QueryParameter("name")]
        String Name { get; }

        /// <summary>
        /// Gets the OID of the asset group
        /// </summary>
        [QueryParameter("oid")]
        String Oid { get; }

    }
    /// <summary>
    /// An interface which defines a generic asset
    /// </summary>
    public interface ICdssAsset 
    {

        /// <summary>
        /// Gets the identifier for the protocol
        /// </summary>
        [QueryParameter("uuid")]
        Guid Uuid { get; }

        /// <summary>
        /// The unique identifier of the object in the scope of the protocol
        /// </summary>
        [QueryParameter("id")]
        String Id { get; }

        /// <summary>
        /// Gets the name of the protocol
        /// </summary>
        [QueryParameter("name")]
        String Name { get; }

        /// <summary>
        /// Gets the version of the protocol
        /// </summary>
        [QueryParameter("version")]
        String Version { get; }

        /// <summary>
        /// Gets the universal OID for this protocol
        /// </summary>
        [QueryParameter("oid")]
        String Oid { get; }

        /// <summary>
        /// Get the documentation
        /// </summary>
        [QueryParameter("annotation")]
        String Documentation { get; }

        /// <summary>
        /// Gets the classification of the object for querying
        /// </summary>
        [QueryParameter("class")]
        CdssAssetClassification Classification { get; }

        /// <summary>
        /// Groups in which the asset belongs
        /// </summary>
        [QueryParameter("group")]
        IEnumerable<ICdssAssetGroup> Groups { get; }

        /// <summary>
        /// Load the protocl definition to <paramref name="definitionStream"/>
        /// </summary>
        void Load(Stream definitionStream);

        /// <summary>
        /// Save the protocol definition to <paramref name="definitionStream"/>
        /// </summary>
        void Save(Stream definitionStream);

    }


    /// <summary>
    /// Represents a protocol asset where the definition is wrapped
    /// </summary>
    /// <remarks>
    /// These wrapped assets allow SanteDB to load the definition of a protocol from one asset format
    /// while storing metadata in another location. For example, the <see cref="Wrapped"/> protocol asset
    /// may be an XML defined clinical protocol, whilst the metadata comes from an HTTP message
    /// </remarks>
    public interface IProtocolAssetWrapper : ICdssAsset
    {

        /// <summary>
        /// Gets the protocol asset which is wrapped
        /// </summary>
        ICdssAsset Wrapped { get; }
    }
}