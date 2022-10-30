using SanteDB.Core.Services;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Service which can encode session id and refresh tokens into an opaque format suitable to be roundtripped through an untrusted context
    /// </summary>
    public interface ISessionTokenEncodingService : IServiceImplementation
    {
        /// <summary>
        /// Encodes a token such that the result is an opaque value that is tamper resistant and suitable for transport through an unsecure context.
        /// </summary>
        /// <param name="token">A token to encode</param>
        /// <returns>A string containing the encoded token.</returns>
        string Encode(byte[] token);
        /// <summary>
        /// Attempts to decode a token. Will return a decoded token in <paramref name="token"/> if the result is true, <c>null</c> otherwise.
        /// </summary>
        /// <param name="encodedToken">An encoded token that was generated through <see cref="Encode(byte[])"/>.</param>
        /// <param name="token">The resulting decoded token when decoding is successful.</param>
        /// <returns><c>true</c> When decoding succeeds, false otherwise.</returns>
        bool TryDecode(string encodedToken, out byte[] token);
        /// <summary>
        /// Attempts to decode a token. Will return the decoded token or throw an exception if the encoded token is invalid.
        /// </summary>
        /// <param name="encodedToken"></param>
        /// <returns></returns>
        byte[] Decode(string encodedToken);
    }
}
