using SanteDB.Core.Diagnostics;
using SanteDB.Core.Http;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{
    [ServiceProvider(SERVICE_NAME, type: ServiceInstantiationType.Singleton)]
    public class OAuthIdentityProvider : IDaemonService
    {
        private const string SERVICE_NAME = "OAuth Identity Provider";

        private readonly Tracer _Tracer;
        readonly IConfigurationManager _ConfigurationManager;
        readonly IServiceManager _ServiceManager;

        private IIdentityProviderService _LocalUserIdentityProvider;
        private IApplicationIdentityProviderService _LocalApplicationIdentityProvider;
        private IDeviceIdentityProviderService _LocalDeviceIdentityProvider;

        private OAuthUserIdentityProvider _OAuthUserProvider;
        private OAuthApplicationIdentityProvider _OAuthApplicationProvider;
        private OAuthDeviceIdentityProvider _OAuthDeviceProvider;

        public OAuthIdentityProvider(IConfigurationManager configurationManager, IServiceManager serviceManager)
        {
            _Tracer = new Tracer(nameof(OAuthIdentityProvider));
            _ConfigurationManager = configurationManager;
            _ServiceManager = serviceManager;
        }


        public string ServiceName => SERVICE_NAME;

        public bool IsRunning { get; private set; }

        public event EventHandler Starting;
        public event EventHandler Started;
        public event EventHandler Stopping;
        public event EventHandler Stopped;

        #region IDaemonService implementation

        //We use this to circumvent the existing identity provider implementations in the system.

        public bool Start()
        {
            Starting?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                var context = ApplicationServiceContext.Current;

                var restclientfactory = context.GetService<IRestClientFactory>();

                var upstreamintegration = context.GetService<IUpstreamIntegrationService>();

                var restclient = restclientfactory?.GetRestClientFor(Interop.ServiceEndpointType.AuthenticationService);

                if (null == restclient)
                {
                    _Tracer.TraceError($"Could not start {nameof(OAuthIdentityProvider)}. No rest service is configured for the Authentication Service.");
                }
                else
                {
                    _LocalUserIdentityProvider = context.GetService<IIdentityProviderService>();
                    _LocalApplicationIdentityProvider = context.GetService<IApplicationIdentityProviderService>();
                    _LocalDeviceIdentityProvider = context.GetService<IDeviceIdentityProviderService>();

                    if (null != _LocalUserIdentityProvider)
                    {
                        _OAuthUserProvider = new OAuthUserIdentityProvider(restclient, _LocalUserIdentityProvider);
                        _ServiceManager.RemoveServiceProvider(_LocalUserIdentityProvider.GetType());
                        _ServiceManager.AddServiceProvider(_OAuthUserProvider);
                    }

                    //if (null != _LocalApplicationIdentityProvider)
                    //{
                    //    _OAuthApplicationProvider = new OAuthApplicationIdentityProvider(restclient, _LocalApplicationIdentityProvider);
                    //    _ServiceManager.RemoveServiceProvider(_LocalApplicationIdentityProvider.GetType());
                    //    _ServiceManager.AddServiceProvider(_OAuthApplicationProvider);
                    //}

                    //if (null != _LocalDeviceIdentityProvider)
                    //{
                    //    _OAuthDeviceProvider = new OAuthDeviceIdentityProvider(restclient, _LocalDeviceIdentityProvider);
                    //    _ServiceManager.RemoveServiceProvider(_LocalDeviceIdentityProvider.GetType());
                    //    _ServiceManager.AddServiceProvider(_OAuthDeviceProvider);

                    //}

                    IsRunning = true;
                }
            };

            Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool Stop()
        {
            Stopping?.Invoke(this, EventArgs.Empty);
            IsRunning = false;

            if (null != _OAuthUserProvider)
            {
                _ServiceManager.RemoveServiceProvider(_OAuthUserProvider.GetType());
                _ServiceManager.AddServiceProvider(_LocalUserIdentityProvider);
                _OAuthUserProvider = null;
            }

            //if (null != _OAuthApplicationProvider)
            //{
            //    _ServiceManager.RemoveServiceProvider(_OAuthApplicationProvider.GetType());
            //    _ServiceManager.AddServiceProvider(_LocalApplicationIdentityProvider);
            //    _OAuthApplicationProvider = null;
            //}

            //if (null != _OAuthDeviceProvider)
            //{
            //    _ServiceManager.RemoveServiceProvider(_OAuthDeviceProvider.GetType());
            //    _ServiceManager.AddServiceProvider(_LocalDeviceIdentityProvider);
            //}

            Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }

        #endregion
    }
}
