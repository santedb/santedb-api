using SanteDB.Core.Model;
using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which is tasked with generating verified pointers to data
    /// </summary>
    public interface IResourcePointerService : IServiceImplementation
    {

        /// <summary>
        /// Generate a structured pointer for the identified object
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="identifer">The list of identifiers to include in the pointer</param>
        /// <returns>The structured pointer</returns>
        String GeneratePointer<TEntity>(IEnumerable<IdentifierBase<TEntity>> identifer)
            where TEntity : VersionedEntityData<TEntity>, new();

        /// <summary>
        /// Resolve the specified resource
        /// </summary>
        /// <param name="pointerData">The pointer to be resolved</param>
        /// <param name="validate">True if validation should be performed</param>
        /// <returns>The resource</returns>
        IdentifiedData ResolveResource(String pointerData, bool validate = false);
    }
}
