using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Attributes
{
    /// <summary>
    /// Instructs the <see cref="X509CertificateUtils"/> class to use a default implementation (overridding the 
    /// detected <see cref="MonoPlatformSecurityProvider"/> or <see cref="DefaultPlatformSecurityProvider"/>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DefaultPlatformSecurityTypeAttribute : Attribute
    {

        /// <summary>
        /// Platform security implementation ctor
        /// </summary>
        /// <param name="platformSecurityImplementation"></param>
        public DefaultPlatformSecurityTypeAttribute(Type platformSecurityImplementation)
        {
            this.PlatformSecurityProviderType = platformSecurityImplementation;
        }

        /// <summary>
        /// Gets the platform security provider type
        /// </summary>
        public Type PlatformSecurityProviderType { get; set; }
    }
}
