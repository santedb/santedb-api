﻿/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Services
{
    
    /// <summary>
    /// Represents a generic interface for business rules services
    /// </summary>
    public interface IBusinessRulesService
    {

        /// <summary>
        /// Gets the next BRE
        /// </summary>
        IBusinessRulesService Next { get; }

        /// <summary>
        /// Called after an insert occurs
        /// </summary>
        Object AfterInsert(Object data);

        /// <summary>
        /// Called after obsolete committed
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Object AfterObsolete(Object data);

        /// <summary>
        /// Called after query
        /// </summary>
        IEnumerable<Object> AfterQuery(IEnumerable<Object> results);

        /// <summary>
        /// Called after retrieve
        /// </summary>
        Object AfterRetrieve(Object result);

        /// <summary>
        /// Called after update committed
        /// </summary>
        Object AfterUpdate(Object data);

        /// <summary>
        /// Called before an insert occurs
        /// </summary>
        Object BeforeInsert(Object data);

        /// <summary>
        /// Called before obsolete
        /// </summary>
        Object BeforeObsolete(Object data);

        /// <summary>
        /// Called before an update occurs
        /// </summary>
        Object BeforeUpdate(Object data);

        /// <summary>
        /// Called to validate a specific object
        /// </summary>
        List<DetectedIssue> Validate(Object data);
    }

    /// <summary>
    /// Represents a service that executes business rules based on triggers which happen in the persistence layer
    /// </summary>
    /// <remarks>
    /// Note: This can be done, instead with events on the persistence layer on the SanteDB datalayer, however there
    /// may come a time when a rule is fired without persistence occurring
    /// </remarks>
    public interface IBusinessRulesService<TModel> : IBusinessRulesService, IServiceImplementation where TModel : IdentifiedData
    {
        /// <summary>
        /// Gets or sets the rule to be run after this rule (for chained rules)
        /// </summary>
        IBusinessRulesService<TModel> Next { get; set; }

        /// <summary>
        /// Called after an insert occurs
        /// </summary>
        TModel AfterInsert(TModel data);

        /// <summary>
        /// Called after obsolete committed
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        TModel AfterObsolete(TModel data);

        /// <summary>
        /// Called after query
        /// </summary>
        IEnumerable<TModel> AfterQuery(IEnumerable<TModel> results);

        /// <summary>
        /// Called after retrieve
        /// </summary>
        TModel AfterRetrieve(TModel result);

        /// <summary>
        /// Called after update committed
        /// </summary>
        TModel AfterUpdate(TModel data);

        /// <summary>
        /// Called before an insert occurs
        /// </summary>
        TModel BeforeInsert(TModel data);

        /// <summary>
        /// Called before obsolete
        /// </summary>
        TModel BeforeObsolete(TModel data);

        /// <summary>
        /// Called before an update occurs
        /// </summary>
        TModel BeforeUpdate(TModel data);

        /// <summary>
        /// Called to validate a specific object
        /// </summary>
        List<DetectedIssue> Validate(TModel data);

    }

}