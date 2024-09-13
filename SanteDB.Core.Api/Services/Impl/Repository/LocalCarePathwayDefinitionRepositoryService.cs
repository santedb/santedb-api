using SanteDB.Core.Model.Acts;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local care pathway definition
    /// </summary>
    public class LocalCarePathwayDefinitionRepositoryService :
        GenericLocalMetadataRepository<CarePathwayDefinition>,
        ICarePathwayDefinitionRepositoryService
    {
        /// <summary>DI constructor</summary>
        public LocalCarePathwayDefinitionRepositoryService(IPolicyEnforcementService policyService, IDataPersistenceService<CarePathwayDefinition> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistence, privacyService)
        {
        }

        /// <inheritdoc/>
        public CarePathwayDefinition GetCarepathDefinition(string mnemonic) => this.Find(o => o.Mnemonic == mnemonic && o.ObsoletionTime == null).FirstOrDefault();
    }
}
