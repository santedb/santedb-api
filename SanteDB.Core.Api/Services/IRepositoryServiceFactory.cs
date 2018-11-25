using SanteDB.Core.Model;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a factory service which can be used to generate default factories
    /// </summary>
    public interface IRepositoryServiceFactory
    {

        /// <summary>
        /// Create the specified resource 
        /// </summary>
        IRepositoryService<T> CreateRepository<T>() where T : IdentifiedData;

    }
}
