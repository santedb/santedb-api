/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-21
 */
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SanteDB.Core.Services.Impl
{

    /// <summary>
    /// Business rule extensions
    /// </summary>
    public static class BusinessRulesExtensions
    {

        /// <summary>
        /// Add a business rule service to this instance of me or the next instance
        /// </summary>
        public static void AddBusinessRuleService(this IServiceProvider me, Type instance)
        {
            var ibre = instance.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IBusinessRulesService<>));
            if (ibre == null)
                throw new InvalidOperationException($"{nameof(instance)} must implement IBusinessRulesService<T>");
            var meth = typeof(BusinessRulesExtensions).GetGenericMethod(nameof(AddBusinessRuleService), ibre.GenericTypeArguments, new Type[] { typeof(IServiceProvider), typeof(Type) });
            meth.Invoke(null, new object[] { me, instance });
        }

        /// <summary>
        /// Adds a new business rule service for the specified model to the application service otherwise adds it to the chain
        /// </summary>
        /// <typeparam name="TModel">The type of model to bind to</typeparam>
        /// <param name="me">The application service to be added to</param>
        /// <param name="breType">The instance of the BRE</param>
        public static void AddBusinessRuleService<TModel>(this IServiceProvider me, Type breType) where TModel : IdentifiedData
        {
            var cbre = me.GetService<IBusinessRulesService<TModel>>();
            if (cbre == null)
                (me as IServiceManager)?.AddServiceProvider(breType);
            else
            {
                while (cbre.Next != null)
                {
                    if (cbre.GetType() == breType) return; // duplicate
                    cbre = cbre.Next;
                }
                cbre.Next = Activator.CreateInstance(breType) as IBusinessRulesService<TModel>;
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
        /// After insert
        /// </summary>
        public virtual TModel AfterInsert(TModel data)
        {
            return this.Next?.AfterInsert(data) ?? data;
        }

        /// <summary>
        /// After obsolete
        /// </summary>
        public virtual TModel AfterObsolete(TModel data)
        {
            return this.Next?.AfterObsolete(data) ?? data;
        }

        /// <summary>
        /// After query
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual IEnumerable<TModel> AfterQuery(IEnumerable<TModel> results)
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
        /// After update
        /// </summary>
        public virtual TModel AfterUpdate(TModel data)
        {
            return this.Next?.AfterUpdate(data) ?? data;
        }

        /// <summary>
        /// Before insert complete
        /// </summary>
        public virtual TModel BeforeInsert(TModel data)
        {
            return this.Next?.BeforeInsert(data) ?? data;
        }

        /// <summary>
        /// Before obselete
        /// </summary>
        public virtual TModel BeforeObsolete(TModel data)
        {
            return this.Next?.BeforeObsolete(data) ?? data;
        }

        /// <summary>
        /// Before update
        /// </summary>
        public virtual TModel BeforeUpdate(TModel data)
        {
            return this.Next?.BeforeUpdate(data) ?? data;
        }

        /// <summary>
        /// Validate the specified object
        /// </summary>
        public virtual List<DetectedIssue> Validate(TModel data)
        {
            return this.Next?.Validate(data) ?? new List<DetectedIssue>();
        }
    }
}