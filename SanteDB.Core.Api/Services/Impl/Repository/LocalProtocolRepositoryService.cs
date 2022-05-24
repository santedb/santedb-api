using SanteDB.Core.i18n;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Protocol;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Default protocol repository services
    /// </summary>
    public class LocalProtocolRepositoryService : GenericLocalRepository<Model.Acts.Protocol>, IClinicalProtocolRepositoryService
    {
        /// <inheritdoc/>
        public LocalProtocolRepositoryService(IPrivacyEnforcementService privacyService, IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<Model.Acts.Protocol> dataPersistence) : base(privacyService, policyService, localizationService, dataPersistence)
        {
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Clinical Protocol Definition Service";

        protected override string DeletePolicy => PermissionPolicyIdentifiers.DeleteClinicalProtocolConfigurationDefinition;
        protected override string AlterPolicy => PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition;
        protected override string WritePolicy => PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition;
        protected override string QueryPolicy => PermissionPolicyIdentifiers.ReadMetadata;
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadMetadata;


        /// <summary>
        /// Find a clinical protocol
        /// </summary>
        [Obsolete("Use FindProtocol(Expression)", true)]
        public IEnumerable<Model.Acts.Protocol> FindProtocol(Expression<Func<Model.Acts.Protocol, bool>> predicate, int offset, int? count, out int totalResults)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Insert a protocol
        /// </summary>
        public IClinicalProtocol InsertProtocol(IClinicalProtocol protocol)
        {
            this.m_policyService.Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition);
            var data = protocol.GetProtocolData();
            if (this.Find(o => o.Oid == data.Oid).Any())
                return protocol.Load( this.Save(data));
            else 
                return protocol.Load(base.Insert(data));
        }

        /// <summary>
        /// Find protocol
        /// </summary>
        public IQueryResultSet<IClinicalProtocol> FindProtocol(Expression<Func<IClinicalProtocol, bool>> predicate)
        => new TransformQueryResultSet<Model.Acts.Protocol, IClinicalProtocol>(this.Find(o => o.ObsoletionTime == null), (a) =>
              {
                  var proto = Activator.CreateInstance(a.HandlerClass) as IClinicalProtocol;
                  proto.Load(a);
                  return proto;
              }).Where(predicate);

        /// <summary>
        /// Get clinical protocol by identifier
        /// </summary>
        public IClinicalProtocol GetProtocol(Guid protocolUuid)
        {
            var protocolData = this.Get(protocolUuid);
            if (protocolData != null)
            {
                var retVal = Activator.CreateInstance(protocolData.HandlerClass) as IClinicalProtocol;
                retVal.Load(protocolData);
                return retVal;
            }
            else
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString(ErrorMessageStrings.NOT_FOUND, new { type = "ClinicalProtocol", id = protocolUuid }));
            }
        }

    }
}
