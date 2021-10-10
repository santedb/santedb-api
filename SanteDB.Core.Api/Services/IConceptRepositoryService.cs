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

using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which is responsible for the maintenance of concepts.
    /// </summary>
    [System.ComponentModel.Description("Concept/Terminology Provider")]
    public interface IConceptRepositoryService : IRepositoryService<Concept>
    {
        /// <summary>
        /// Searches for a concept by name and language.
        /// </summary>
        /// <param name="name">The name of the concept.</param>
        /// <param name="language">The language of the concept.</param>
        /// <returns>Returns a list of concepts.</returns>
        IEnumerable<Concept> FindConceptsByName(string name, string language);

        /// <summary>
        /// Finds a concept by reference term.
        /// </summary>
        /// <param name="code">The code of the reference term.</param>
        /// <param name="codeSystem">The code system OID of the reference term.</param>
        /// <returns>Returns a list of concepts.</returns>
        IEnumerable<ConceptReferenceTerm> FindConceptsByReferenceTerm(string code, Uri codeSystem);

        /// <summary>
        /// Get concept set members
        /// </summary>
        IEnumerable<Concept> GetConceptSetMembers(string mnemonic);

        /// <summary>
        /// Finds a concept by reference term only where the concept is equivalent
        /// </summary>
        /// <param name="code">The code of the reference term.</param>
        /// <param name="codeSystemDomain">The code system OID of the reference term.</param>
        /// <returns>Returns a list of concepts.</returns>
        Concept GetConceptByReferenceTerm(string code, String codeSystemDomain);

        /// <summary>
        /// Finds a concept by reference term.
        /// </summary>
        /// <param name="code">The code of the reference term.</param>
        /// <param name="codeSystemDomain">The code system OID of the reference term.</param>
        /// <returns>Returns a list of concepts.</returns>
        IEnumerable<ConceptReferenceTerm> FindConceptsByReferenceTerm(string code, String codeSystemDomain);

        /// <summary>
        /// Gets a concept by mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic of the concept.</param>
        /// <returns>Returns the concept.</returns>
        Concept GetConcept(string mnemonic);

        /// <summary>
        /// Returns a value which indicates whether <paramref name="a"/> implies <paramref name="b"/>
        /// </summary>
        /// <param name="a">The left hand concept.</param>
        /// <param name="b">The right hand concept.</param>
        /// <returns>Returns true if the first concept implies the second concept.</returns>
        bool Implies(Concept a, Concept b);

        /// <summary>
        /// Returns true if the concept <paramref name="concept"/> is a member of set <paramref name="set"/>
        /// </summary>
        bool IsMember(ConceptSet set, Concept concept);

        /// <summary>
        /// Returns true if the concept <paramref name="concept"/> is a member of set <paramref name="set"/>
        /// </summary>
        bool IsMember(Guid set, Guid concept);

        /// <summary>
        /// Gets the concept reference term for the specified code system
        /// </summary>
        /// <returns></returns>
        ReferenceTerm GetConceptReferenceTerm(Guid conceptId, String codeSystem, bool exact = true);

        /// <summary>
        /// Gets the concept reference term for the specified code system
        /// </summary>
        /// <returns></returns>
        ReferenceTerm GetConceptReferenceTerm(String conceptMnemonic, String codeSystem, bool exact = true);

        /// <summary>
        /// Gets the concept reference term for the specified code system
        /// </summary>
        /// <returns></returns>
        IEnumerable<ConceptReferenceTerm> FindReferenceTermsByConcept(Guid conceptId, String codeSystem);
    }
}