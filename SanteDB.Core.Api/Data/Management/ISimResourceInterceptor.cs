using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Management
{
    /// <summary>
    /// Implementers claim to provide SIM resource interception
    /// </summary>
    internal interface ISimResourceInterceptor : IDisposable
    {
        /// <summary>
        /// Perform any matching and persistence checking logic
        /// </summary>
        /// <param name="inputRecord">The record which is being inserted</param>
        IEnumerable<IdentifiedData> DoMatchingLogic(IdentifiedData inputRecord);

        /// <summary>
        /// Perform any deletion logic
        /// </summary>
        /// <param name="inputRecord">The record whihc is being deleted</param>
        IEnumerable<IdentifiedData> DoDeletionLogic(IdentifiedData inputRecord);
    }
}