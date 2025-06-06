﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Business rule extensions
    /// </summary>
    public static class BusinessRulesExtensions
    {
        /// <summary>
        /// Add a business rule service to this instance of me or the next instance
        /// </summary>
        public static object AddBusinessRule(this IServiceProvider me, Type instance)
        {
            var ibre = instance.FindInterfaces((t, p) => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(IBusinessRulesService<>), null).FirstOrDefault();
            if (ibre == null)
            {
                throw new InvalidOperationException($"{nameof(instance)} must implement IBusinessRulesService<T>");
            }

            var meth = typeof(BusinessRulesExtensions).GetGenericMethod(nameof(AddBusinessRule), ibre.GenericTypeArguments, new Type[] { typeof(IServiceProvider), typeof(Type) });
            return meth.Invoke(null, new object[] { me, instance });
        }

        /// <summary>
        /// Add a business rule service to this instance of me or the next instance
        /// </summary>
        public static IBusinessRulesService GetBusinessRuleService(this IServiceProvider me, Type instanceType)
        {
            var ibt = typeof(IBusinessRulesService<>).MakeGenericType(instanceType);
            return ApplicationServiceContext.Current.GetService(ibt) as IBusinessRulesService;
        }

        /// <summary>
        /// Adds a new business rule service for the specified model to the application service otherwise adds it to the chain
        /// </summary>
        /// <typeparam name="TModel">The type of model to bind to</typeparam>
        /// <param name="me">The application service to be added to</param>
        /// <param name="breType">The instance of the BRE</param>
        public static object AddBusinessRule<TModel>(this IServiceProvider me, Type breType) where TModel : IdentifiedData
        {
            var cbre = me.GetService<IBusinessRulesService<TModel>>();
            var serviceManager = me.GetService<IServiceManager>();
            if (cbre == null)
            {
                serviceManager?.AddServiceProvider(breType);
                return me.GetService(breType);
            }
            else if (cbre.GetType() != breType) // Only add if different
            {
                while (cbre.Next != null)
                {
                    if (cbre.GetType() == breType)
                    {
                        return breType; // duplicate
                    }

                    cbre = cbre.Next;
                }
                cbre.Next = serviceManager.CreateInjected(breType) as IBusinessRulesService<TModel>;
                return cbre.Next;
            }
            else
            {
                return cbre;
            }
        }
    }

    /// <summary>
    /// Represents a business rules service with no behavior, but intended to help in the implementation of another
    /// business rules service
    /// </summary>
    public abstract class BaseBusinessRulesService<TModel> : IBusinessRulesService<TModel> where TModel : IdentifiedData
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => $"Business rules for {typeof(TModel).FullName}";

        /// <summary>
        /// Gets the next behavior
        /// </summary>
        public IBusinessRulesService<TModel> Next { get; set; }

        /// <summary>
        /// Next
        /// </summary>
        IBusinessRulesService IBusinessRulesService.Next => this.Next;

        /// <summary>
        /// After insert
        /// </summary>
        public virtual TModel AfterInsert(TModel data)
        {
            return this.Next?.AfterInsert(data) ?? data;
        }

        /// <summary>
        /// After object is inserted
        /// </summary>
        public object AfterInsert(object data)
        {
            return this.AfterInsert((TModel)data);
        }

        /// <summary>
        /// After obsolete
        /// </summary>
        public virtual TModel AfterDelete(TModel data)
        {
            return this.Next?.AfterDelete(data) ?? data;
        }

        /// <summary>
        /// After obsoletion
        /// </summary>
        public object AfterObsolete(object data)
        {
            return this.AfterDelete((TModel)data);
        }

        /// <summary>
        /// After query
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual IQueryResultSet<TModel> AfterQuery(IQueryResultSet<TModel> results)
        {
            return this.Next?.AfterQuery(results) ?? results;
        }

        /// <summary>
        /// After query
        /// </summary>
        public IQueryResultSet AfterQuery(IQueryResultSet results)
        {
            return this.Next?.AfterQuery(results) ?? results;
        }

        /// <summary>
        /// Fired after retrieve
        /// </summary>
        public virtual TModel AfterRetrieve(TModel result)
        {
            return this.Next?.AfterRetrieve(result) ?? result;
        }

        /// <summary>
        /// After the data has been retrieved
        /// </summary>
        public object AfterRetrieve(object result)
        {
            return this.AfterRetrieve((TModel)result);
        }

        /// <summary>
        /// After update
        /// </summary>
        public virtual TModel AfterUpdate(TModel data)
        {
            return this.Next?.AfterUpdate(data) ?? data;
        }

        /// <summary>
        /// After update
        /// </summary>
        public object AfterUpdate(object data)
        {
            return this.AfterUpdate((TModel)data);
        }

        /// <summary>
        /// Before insert complete
        /// </summary>
        public virtual TModel BeforeInsert(TModel data)
        {
            return this.Next?.BeforeInsert(data) ?? data;
        }

        /// <summary>
        /// Before insert
        /// </summary>
        public object BeforeInsert(object data)
        {
            return this.BeforeInsert((TModel)data);
        }

        /// <summary>
        /// Before obselete
        /// </summary>
        public virtual TModel BeforeDelete(TModel data)
        {
            return this.Next?.BeforeDelete(data) ?? data;
        }

        /// <summary>
        /// Before obsoletion occurs
        /// </summary>
        public object BeforeObsolete(object data)
        {
            return this.BeforeDelete((TModel)data);
        }

        /// <summary>
        /// Before update
        /// </summary>
        public virtual TModel BeforeUpdate(TModel data)
        {
            return this.Next?.BeforeUpdate(data) ?? data;
        }

        /// <summary>
        /// Before update occurs
        /// </summary>
        public object BeforeUpdate(object data)
        {
            return this.BeforeUpdate((TModel)data);
        }

        /// <summary>
        /// Validate the specified object
        /// </summary>
        public virtual List<DetectedIssue> Validate(TModel data)
        {
            return this.Next?.Validate(data) ?? new List<DetectedIssue>();
        }

        /// <summary>
        /// Validate the specified object
        /// </summary>
        public List<DetectedIssue> Validate(object data)
        {
            return this.Validate((TModel)data);
        }
    }
}