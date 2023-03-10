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
 * Date: 2023-3-10
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Entity lookup 
    /// </summary>
    public class EntityLookup : IForeignDataElementTransform
    {

        // Serialization binder
        private readonly Regex m_parmExtract = new Regex(@"\$(\w*?)[&\.\?]", RegexOptions.Compiled);
        private readonly ModelSerializationBinder m_serialization = new ModelSerializationBinder();
        private readonly IAdhocCacheService m_adhocCache;

        /// <summary>
        /// DI constructor
        /// </summary>
        public EntityLookup(IAdhocCacheService adhocCache  = null)
        {
            this.m_adhocCache = adhocCache;
        }

        /// <summary>
        /// Entity lookup transform
        /// </summary>
        public string Name => "EntityLookup";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            if(args.Length != 2 && args.Length != 3)
            {
                throw new ArgumentOutOfRangeException("arg2", "Missing arguments");
            }
            var modelType = this.m_serialization.BindToType(typeof(Person).Assembly.FullName, args[0].ToString());
            var lookupRepoType = typeof(IRepositoryService<>).MakeGenericType(modelType);
            var lookupRepo = ApplicationServiceContext.Current.GetService(lookupRepoType) as IRepositoryService;
            var parms = new Dictionary<String, Func<Object>>();
            for (int i = 0; i < sourceRecord.ColumnCount; i++)
            {
                var parmNo = i;
                parms.Add(sourceRecord.GetName(i), () => sourceRecord[parmNo]);
            }
            parms.Add("input", () => input);

            var key = this.m_parmExtract.Replace($"lu.{args[0]}.{args[1]}?{input}", o=>
            {
                if(parms.TryGetValue(o.Groups[1].Value, out var fn))
                {
                    return fn().ToString();
                }
                return String.Empty;
            });
            var result = this.m_adhocCache?.Get<Guid?>(key);
            if (result == null)
            {

                
                var keySelector = QueryExpressionParser.BuildPropertySelector(modelType, "id", false, typeof(Guid?));
                result = lookupRepo.Find(QueryExpressionParser.BuildLinqExpression(modelType, args[1].ToString().ParseQueryString(), "o", parms, lazyExpandVariables: false)).Select<Guid?>(keySelector).SingleOrDefault();
                this.m_adhocCache?.Add(key, result ?? Guid.Empty);

            }

            if(args.Length == 3 && result.GetValueOrDefault() != Guid.Empty)
            {
                // TODO: Cache these expressions
                var keySelector = QueryExpressionParser.BuildPropertySelector(modelType, args[2].ToString(), false, typeof(object));
                var obj = lookupRepo.Get(result.Value) as IdentifiedData;
                return keySelector.Compile().DynamicInvoke(obj);
            }
            else
            {
                if(result.GetValueOrDefault() == Guid.Empty)
                {
                    return null;
                }
                return result;
            }
        }
    }
}
