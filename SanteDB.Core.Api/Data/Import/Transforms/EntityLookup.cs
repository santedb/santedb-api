using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Entity lookup 
    /// </summary>
    public class EntityLookup : IForeignDataElementTransform
    {

        // Serialization binder
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
        public object Transform(object input, params object[] args)
        {
            if(args.Length != 2)
            {
                throw new ArgumentOutOfRangeException("arg2", "Missing arguments");
            }

            var key = $"lu.{args[0]}.{args[1]}?{input}";
            var result = this.m_adhocCache?.Get<Guid?>(key);
            if (result != null)
            {
                return result == Guid.Empty ? null : result;
            }
            else
            {
                var modelType = this.m_serialization.BindToType(typeof(Person).Assembly.FullName, args[0].ToString());
                var lookupRepoType = typeof(IRepositoryService<>).MakeGenericType(modelType);
                var lookupRepo = ApplicationServiceContext.Current.GetService(lookupRepoType) as IRepositoryService;
                var keySelector = QueryExpressionParser.BuildPropertySelector(modelType, "id", false, typeof(Guid?));
                result = lookupRepo.Find(QueryExpressionParser.BuildLinqExpression(modelType, args[1].ToString().ParseQueryString(), "o", new Dictionary<string, Func<object>>()
                {
                    {  "input", () => input }
                })).Select<Guid?>(keySelector).SingleOrDefault();

                this.m_adhocCache?.Add(key, result ?? Guid.Empty);
                return result;
            }

        }
    }
}
