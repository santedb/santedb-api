using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local entity repository 
    /// </summary>
    public class LocalEntityRepository : GenericLocalRepositoryEx<Entity>
    {
        /// <summary>
        /// Local entity repository
        /// </summary>
        public LocalEntityRepository(IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<Entity> dataPersistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, localizationService, dataPersistenceService, privacyService)
        {
        }

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadMetadata;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string WritePolicy => PermissionPolicyIdentifiers.Login;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.UnrestrictedMetadata;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.Login;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string QueryPolicy => PermissionPolicyIdentifiers.ReadMetadata;

        /// <inheritdoc/>
        public override Entity Delete(Guid key)
        {
            var data = this.Get(key);
            var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
            var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as ISecuredRepositoryService;
            otherRepository.DemandDelete(key);
            return otherRepository.Delete(key) as Entity; // Let the other repository decide what to do 
        }

        /// <inheritdoc/>
        public override Entity Save(Entity data)
        {
            var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
            var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as IRepositoryService;
            return otherRepository.Save(data) as Entity; // Let the other repository decide what to do 
        }

        /// <inheritdoc/>
        public override Entity Insert(Entity data)
        {
            var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
            var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as IRepositoryService;
            return otherRepository.Insert(data) as Entity; // Let the other repository decide what to do 
        }

        /// <inheritdoc/>
        public override Entity Get(Guid key, Guid versionKey)
        {
            var data = base.Get(key, versionKey);
            // Allow the other to demand its permissions
            var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
            var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as ISecuredRepositoryService;
            otherRepository.DemandRead(key);
            return data;
        }

        /// <inheritdoc/>
        public override IQueryResultSet<Entity> Find(Expression<Func<Entity, bool>> query)
        {
            return new NestedQueryResultSet<Entity>(base.Find(query), (o) =>
            {
                var repositoryType = typeof(IRepositoryService<>).MakeGenericType(o.GetType());
                var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as ISecuredRepositoryService;
                otherRepository.DemandQuery();
                return o;
            });
        }

    }
}
