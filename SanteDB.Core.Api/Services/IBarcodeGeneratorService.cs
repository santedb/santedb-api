using SanteDB.Core.Model;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Api.Services
{
    /// <summary>
    /// Represents a barcode generator
    /// </summary>
    public interface IBarcodeGeneratorService : IServiceImplementation
    {

        /// <summary>
        /// Generate a barcode from the specified identifier
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to which the identifier is attached</typeparam>
        Stream Generate<TEntity>(IEnumerable<IdentifierBase<TEntity>> identifers)
            where TEntity : VersionedEntityData<TEntity>, new();

    }
}
