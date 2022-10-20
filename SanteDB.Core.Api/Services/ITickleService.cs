using SanteDB.Core.Model.Tickles;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which provides tickles for the user (popup messages)
    /// </summary>
    public interface ITickleService : IServiceImplementation
    {
        /// <summary>
        /// Dismiss a tickle
        /// </summary>
        void DismissTickle(Guid tickleId);

        /// <summary>
        /// Send a tickle to the user screen
        /// </summary>
        void SendTickle(Tickle tickle);

        /// <summary>
        /// Get tickles
        /// </summary>
        IEnumerable<Tickle> GetTickles(Expression<Func<Tickle, bool>> filter);
    }
}
