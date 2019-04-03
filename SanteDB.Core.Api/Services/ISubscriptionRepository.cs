using SanteDB.Core.Model.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents an implementation of a repository which loads subscription definitions
    /// </summary>
    public interface ISubscriptionRepository : IRepositoryService<SubscriptionDefinition>
    {
    }
}
