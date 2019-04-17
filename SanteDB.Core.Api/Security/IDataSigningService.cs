using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a service which can sign arbitrary data 
    /// </summary>
    public interface IDataSigningService : IServiceImplementation
    {

        /// <summary>
        /// Signs the specified data using the service's configured signing key
        /// </summary>
        /// <param name="data">The data to be signed</param>
        /// <param name="keyId">The numeric identifier of the key to use</param>
        /// <returns>The digital signature</returns>
        byte[] SignData(byte[] data, string keyId = null);

        /// <summary>
        /// Verifies the digital signature of the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature">The digital signature to be verified</param>
        /// <param name="keyId">The identifier of the key to use for verification</param>
        /// <returns>True if the signature is valid</returns>
        bool Verify(byte[] data, byte[] signature, string keyId = null);
    }
}
