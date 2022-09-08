using SanteDB.Core.Model;
using SanteDB.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Represents an exception with the CDSS engine
    /// </summary>
    public class CdssException : Exception
    {

        /// <summary>
        /// The name of the protocol data element 
        /// </summary>
        public const string ProtocolDataName = "protocols";
        /// <summary>
        /// The name of the target data element
        /// </summary>
        public const string TargetDataName = "target";

        /// <summary>
        /// Gets the protocols which caused the exception
        /// </summary>
        public IEnumerable<IClinicalProtocol> Protocols => this.Data[ProtocolDataName] as IEnumerable<IClinicalProtocol>;

        /// <summary>
        /// Gets the target which caused the exception
        /// </summary>
        public String Target => this.Data[TargetDataName].ToString();

        /// <summary>
        /// CDSS exception
        /// </summary>
        /// <param name="protocols">The protocols which were applied</param>
        /// <param name="target">The target of the CDSS call</param>
        /// <param name="cause">The cause of the exception</param>
        public CdssException(IEnumerable<IClinicalProtocol> protocols, IdentifiedData target, Exception cause) : base($"Error executing CDSS rules against {target}", cause)
        {
            this.Data.Add(ProtocolDataName, protocols);
            this.Data.Add(TargetDataName, target.ToString());
        }

        /// <summary>
        /// Creates a new instance of the CDSS exception
        /// </summary>
        /// <param name="protocols">The clinical protocols that were attempted to be applied</param>
        /// <param name="target">The target of the CDSS operation</param>
        public CdssException(IEnumerable<IClinicalProtocol> protocols, IdentifiedData target) : this(protocols, target, null)
        {

        }
    }
}
