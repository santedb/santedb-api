using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Indicates that the upstream realm has changed
    /// </summary>
    public class UpstreamRealmChangedEventArgs : EventArgs
    {

        /// <summary>
        /// Get the upstream integration service
        /// </summary>
        public IUpstreamIntegrationService UpstreamIntegrationService { get; }

        /// <summary>
        /// Create new realm change event args
        /// </summary>
        public UpstreamRealmChangedEventArgs(IUpstreamIntegrationService upstreamIntegrationService)
        {
            this.UpstreamIntegrationService = upstreamIntegrationService;
        }
    }

    /// <summary>
    /// Represents an upstream enrolment management service
    /// </summary>
    public interface IUpstreamManagementService : IServiceImplementation
    {

        /// <summary>
        /// The realm settings have changed.
        /// </summary>
        event EventHandler<UpstreamRealmChangedEventArgs> RealmChanged;

        /// <summary>
        /// Joins the specified <paramref name="targetRealm"/>
        /// </summary>
        /// <param name="targetRealm">The target realm to join</param>
        void Join(IUpstreamRealmSettings targetRealm);

        /// <summary>
        /// Determines if the upstream has been configured
        /// </summary>
        /// <returns>True if the upstream is configured</returns>
        bool IsConfigured();

        /// <summary>
        /// Gets the upstream realm
        /// </summary>
        /// <returns></returns>
        IUpstreamRealmSettings GetSettings();

        /// <summary>
        /// Un-joins the upstream target realm
        /// </summary>
        void UnJoin();


    }
}
