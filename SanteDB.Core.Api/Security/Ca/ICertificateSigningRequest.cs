namespace SanteDB.Core.Security.Ca
{
    /// <summary>
    /// Represents a certificate signing request in a method which is abstracted from implementation
    /// </summary>
    public interface ICertificateSigningRequest
    {

        /// <summary>
        /// Gets the ID of the signing request
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the status of the signing request
        /// </summary>
        CertificateSigningRequestStatus Status { get; }

    }
}