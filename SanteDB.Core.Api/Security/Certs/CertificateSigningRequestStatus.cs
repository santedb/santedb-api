namespace SanteDB.Core.Security.Certs
{
    /// <summary>
    /// The certificate signing request status
    /// </summary>
    public enum CertificateSigningRequestStatus
    {

        /// <summary>
        /// The CSR is pending approval
        /// </summary>
        Pending,
        /// <summary>
        /// The CSR was approved and a certificate issued
        /// </summary>
        Approved,
        /// <summary>
        /// The CSR was rejected
        /// </summary>
        Rejected

    }
}