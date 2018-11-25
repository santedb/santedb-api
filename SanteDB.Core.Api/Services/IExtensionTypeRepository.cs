using SanteDB.Core.Model.DataTypes;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents the extension type repository
    /// </summary>
    public interface IExtensionTypeRepository : IRepositoryService<ExtensionType>
    {

        /// <summary>
        /// Get the xtension type my its url
        /// </summary>
        ExtensionType Get(Uri uri);

    }
}
