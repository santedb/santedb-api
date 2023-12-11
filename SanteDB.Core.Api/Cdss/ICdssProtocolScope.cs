using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents an asset grouping
    /// </summary>
    public interface ICdssProtocolScope
    {
        /// <summary>
        /// Gets the ID of the asset group
        /// </summary>
        [QueryParameter("uuid")]
        Guid Uuid { get; }

        /// <summary>
        /// Gets the name of the asset group
        /// </summary>
        [QueryParameter("name")]
        String Name { get; }

        /// <summary>
        /// Gets the OID of the asset group
        /// </summary>
        [QueryParameter("oid")]
        String Oid { get; }

    }
}
