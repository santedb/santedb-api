using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Manages streams of data for other services 
    /// </summary>
    public interface IDataStreamManager
    {

        /// <summary>
        /// Gets the stream data by identifier from the stream manager
        /// </summary>
        /// <param name="streamId">The id of the stream to get</param>
        /// <returns>The stream loaded from the backing store</returns>
        Stream Get(Guid streamId);

        /// <summary>
        /// Add a stream to the stream manager
        /// </summary>
        /// <param name="stream">The stream to be added</param>
        /// <returns>The stream identifier assigned to the stream</returns>
        Guid Add(Stream stream);

        /// <summary>
        /// Deletes a stream from the stream manager
        /// </summary>
        /// <param name="streamId">The identifier of the stream to remove</param>
        void Remove(Guid streamId);
    }
}
