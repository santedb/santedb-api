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
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which is responsible for the maintenance of concepts.
    /// </summary>
    /// <remarks>
    /// <para>This class is responsible for the management of <see cref="Concept"/>, <see cref="ConceptSet"/>, and <see cref="ReferenceTerm"/> definitions
    /// from the <see href="https://help.santesuite.org/santedb/data-and-information-architecture/conceptual-data-model/concept-dictionary">SanteDB CDR's concept dictionary</see>. The
    /// implementation of this service contract should provide methods for contacting the storage provider (either local database or a remote terminology service)
    /// to:</para>
    /// <list type="bullet">
    ///     <item>Resolve <see cref="ReferenceTerm"/> instances from inbound messages from code/system pairs</item>
    ///     <item>Resolve appropriate <see cref="ReferenceTerm"/> data given a <see cref="Concept"/> instance from the SanteDB CDR to be sent on an outbound message</item>
    ///     <item>Determine the membership of a <see cref="Concept"/> in a <see cref="ConceptSet"/></item>
    ///     <item>Determiner relationships between <see cref="Concept"/> instances</item>
    /// </list>
    /// </remarks>
    [System.ComponentModel.Description("Concept/Terminology Provider")]
    public interface IConceptRepositoryService : IRepositoryService<Concept>
    {
        /// <summary>
        /// Searches for a <see cref="Concept"/> by name and language.
        /// </summary>
        /// <param name="name">The name of the concept</param>
        /// <param name="language">The language of the concept</param>
        /// <returns>Returns a list of <see cref="Concept"/> which have the specified <paramref name="name"/></returns>
        IEnumerable<Concept> FindConceptsByName(string name, string language);

        /// <summary>
        /// Finds a concept by reference term information, returning the <see cref="ConceptReferenceTerm"/> 
        /// so the caller can determine if the <see cref="Concept"/> and <see cref="ReferenceTerm"/> are equivalent,
        /// narrower than, etc.
        /// </summary>
        /// <param name="code">The code mnemonic of the reference term</param>
        /// <param name="codeSystem">The code system OID or URL for the reference term</param>
        /// <returns>Returns a list of <see cref="ConceptReferenceTerm"/> relationships</returns>
        IEnumerable<ConceptReferenceTerm> FindConceptsByReferenceTerm(string code, Uri codeSystem);

        /// <summary>
        /// Gets all the <see cref="Concept"/> members of the specified concept set
        /// </summary>
        /// <param name="mnemonic">The mnemonic of the concept set</param>
        /// <returns>All members of the concept set</returns>
        IEnumerable<Concept> GetConceptSetMembers(string mnemonic);

        /// <summary>
        /// Finds a concept by reference term only where the concept is equivalent to the reference term
        /// </summary>
        /// <param name="code">The code mnemonic of the reference term (what was received on the wire) </param>
        /// <param name="codeSystemDomain">The code system mnemonic, OID or domain name in which the reference term resides</param>
        /// <returns>The internal SanteDB <see cref="Concept"/> which is equivalent (null if no equivalent could be found)</returns>
        Concept GetConceptByReferenceTerm(string code, String codeSystemDomain);

        /// <summary>
        /// Finds all <see cref="Concept"/> instances which are associated with the specified <see cref="ReferenceTerm"/>
        /// </summary>
        /// <param name="code">The code mnemonic of the reference term</param>
        /// <param name="codeSystemDomain">The code system domain name, OID or URL of the reference term.</param>
        /// <returns>Returns a list of concept relationships where the <see cref="Concept"/> is related to a reference term with 
        /// <paramref name="code"/> in <paramref name="codeSystemDomain"/></returns>
        IEnumerable<ConceptReferenceTerm> FindConceptsByReferenceTerm(string code, String codeSystemDomain);

        /// <summary>
        /// Get a <see cref="Concept"/> instance given the concept's unique mnemonic
        /// </summary>
        /// <param name="mnemonic">The mnemonic of the concept</param>
        /// <returns>The concept which carries the specified <paramref name="mnemonic"/></returns>
        Concept GetConcept(string mnemonic);

        /// <summary>
        /// Returns a value which indicates whether concept <paramref name="a"/> implies concept <paramref name="b"/> through 
        /// a <see cref="ConceptRelationship"/> indicating the two are the same
        /// </summary>
        /// <param name="a">The concept which implication is being tested</param>
        /// <param name="b">The second concept for which implication is being tested</param>
        /// <returns>Returns true if the first concept implies the second concept.</returns>
        bool Implies(Concept a, Concept b);

        /// <summary>
        /// Returns true if the concept <paramref name="concept"/> is a member of set <paramref name="set"/>
        /// </summary>
        /// <param name="concept">The <see cref="Concept"/> to test for membership</param>
        /// <param name="set">The <see cref="ConceptSet"/> to test for membership of <paramref name="concept"/></param>
        bool IsMember(ConceptSet set, Concept concept);

        /// <summary>
        /// Returns true if the concept <paramref name="concept"/> is a member of set <paramref name="set"/>
        /// </summary>
        /// <seealso cref="IsMember(ConceptSet, Concept)"/>
        bool IsMember(Guid set, Guid concept);

        /// <summary>
        /// Gets the concept reference term for the specified code system
        /// </summary>
        /// <param name="codeSystem">The name of the code system (domain name, OID or URL)</param>
        /// <param name="conceptId">The identifier of the concept to retrieve the reference term</param>
        /// <param name="exact">True if the <see cref="ReferenceTerm"/> returned must be the same as the (i.e. equivalent and not narrower or greater than) <paramref name="conceptId"/></param>
        /// <returns>The reference term (if any)</returns>
        ReferenceTerm GetConceptReferenceTerm(Guid conceptId, String codeSystem, bool exact = true);

        /// <summary>
        /// Gets the concept reference term for the <see cref="Concept"/> with <paramref name="conceptMnemonic"/> in <paramref name="codeSystem"/>
        /// only if the <see cref="ReferenceTerm"/> is the same in scope as the <see cref="Concept"/>
        /// </summary>
        /// <returns></returns>
        ReferenceTerm GetConceptReferenceTerm(String conceptMnemonic, String codeSystem);

        /// <summary>
        /// Finds all reference terms for the concept with <paramref name="conceptId"/> in the specified <paramref name="codeSystem"/> system.
        /// </summary>
        /// <param name="codeSystem">The code system oid, url or domain name for which the reference terms should be loaded</param>
        /// <param name="conceptId">The identifier of the concept</param>
        /// <returns>All <see cref="ReferenceTerm"/> instances (and their relationship strength)</returns>
        IEnumerable<ConceptReferenceTerm> FindReferenceTermsByConcept(Guid conceptId, String codeSystem);


        /// <summary>
        /// Get the specified concept name
        /// </summary>
        string GetName(Guid conceptId, string twoLetterISOLanguageName);

        /// <summary>
        /// Expand the concept set to a flat list of values
        /// </summary>
        /// <param name="conceptSetId">The concept set identifier</param>
        /// <returns>The concepts in the concept set</returns>
        IQueryResultSet<Concept> ExpandConceptSet(Guid conceptSetId);

        /// <summary>
        /// Expand the concept set by name
        /// </summary>
        /// <param name="conceptSetMnemonic">The concept set mnemonic</param>
        /// <returns>The list of concepts</returns>
        IQueryResultSet<Concept> ExpandConceptSet(String conceptSetMnemonic);
    }
}