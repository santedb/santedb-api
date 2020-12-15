using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Data Archive service
    /// </summary>
    public interface IDataArchiveService
    {
        /// <summary>
        /// Push the specified records to the archive
        /// </summary>
        void Archive(Type modelType, params Guid[] keysToBeArchived);

        /// <summary>
        /// Retrieve a record from the archive by key and type
        /// </summary>
        IdentifiedData Retrieve(Type modelType, Guid keyToRetrieve);

        /// <summary>
        /// Validates whether the specified key exists in the archive
        /// </summary>
        bool Exists(Type modelType, Guid keyToCheck);

        /// <summary>
        /// Purge the specified object from the archive
        /// </summary>
        void Purge(Type modelType, params Guid[] keysToBePurged);

    }

}
