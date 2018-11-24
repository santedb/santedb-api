using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
