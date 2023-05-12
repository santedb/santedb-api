using SanteDB.Core.Data.Initialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// An implementation of a <see cref="IForeignDataReader"/> which can produce a bulk <see cref="Dataset"/>
    /// </summary>
    public interface IForeignDataBulkReader
    {

        /// <summary>
        /// Read the contents as a dataset
        /// </summary>
        Dataset ReadAsDataset();
    }
}
