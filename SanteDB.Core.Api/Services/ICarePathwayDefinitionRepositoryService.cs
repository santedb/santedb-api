using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// CAre pathway definition service
    /// </summary>
    public interface ICarePathwayDefinitionRepositoryService : IRepositoryService<CarePathwayDefinition>
    {

        /// <summary>
        /// Get the specified care pathway which has the mnemonic 
        /// </summary>
        CarePathwayDefinition GetCarepathDefinition(String mnemonic);

    }
}
