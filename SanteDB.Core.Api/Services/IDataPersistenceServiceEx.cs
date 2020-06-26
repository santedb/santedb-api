using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Extended data persistence service
    /// </summary>
    public interface IDataPersistenceServiceEx<TModel> : IDataPersistenceService<TModel>
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Touch the specified data
        /// </summary>
        void Touch(Guid key, TransactionMode mode, IPrincipal principal);

    }
}
