/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-3-10
 */
using RestSrvr;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// The core implementation of <see cref="IServiceProvider"/> and <see cref="IServiceManager"/> 
    /// that supports SanteDB's <see href="https://help.santesuite.org/developers/server-plugins/service-definitions#dependency-injection">dependency injection</see>
    /// technology.
    /// </summary>
    /// <remarks>
    /// <para>The dependency injection service manager is responsible for:</para>
    /// <list type="bullet">
    ///     <item>Maintaining singleton or per-call instances registered in the <see cref="ApplicationServiceContextConfigurationSection"/></item>
    ///     <item>Determining the dependencies of each service via <c>CreateInjected()</c> and ensuring they exist and are constructed for injection</item>
    ///     <item>Validating the digital signatures on assembly files which are used by the SanteDB system (see: <see href="https://help.santesuite.org/developers/server-plugins/digital-signing-requirements">Digital Signing Requirements</see>)</item>
    ///     <item>Calling any <see cref="IServiceFactory"/> instance to attempt to construct missing services</item>
    ///     <item>Coordinating the lifecycle of <see cref="IDaemonService"/> instances</item>
    /// </list>
    /// <para>Note: You must have an <see cref="IConfigurationManager"/> instance registered in the application service context prior to calling the <c>Start()</c> method on this class</para>
    /// </remarks>
    public class DependencyServiceManager : IServiceManager, IServiceProvider, IDaemonService, IDisposable, IReportProgressChanged
    {
        // DI Stack
        private ThreadLocal<Stack<Type>> m_dependencyInjectionStack = new ThreadLocal<Stack<Type>>();

        // Activators
        private ConcurrentDictionary<Type, Func<Object>> m_activators = new ConcurrentDictionary<Type, Func<object>>();

        // Not configured services
        private HashSet<Type> m_notConfiguredServices = new HashSet<Type>();

        // Service factories
        private HashSet<IServiceFactory> m_serviceFactories = new HashSet<IServiceFactory>();


        /// <summary>
        /// Gets the service instance information
        /// </summary>
        private class ServiceInstanceInformation : IDisposable
        {
            // Tracer
            private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ServiceInstanceInformation));

            // Singleton instance
            private object m_singletonInstance = null;

            // Lock object
            private object m_lockBox = new object();


            // Service manager
            private DependencyServiceManager m_serviceManager;

            // Implemented services
            private HashSet<Type> m_implementedServices;

            // Dependent service
            private Type[] m_injectedServices;

            /// <summary>
            /// Create a new service instance
            /// </summary>
            private ServiceInstanceInformation(DependencyServiceManager serviceManager)
            {
                this.m_serviceManager = serviceManager;
            }

            /// <summary>
            /// Gets the service instance information
            /// </summary>
            public ServiceInstanceInformation(Type serviceImplementationClass, DependencyServiceManager serviceManager) : this(serviceManager)
            {
                this.ServiceImplementer = serviceImplementationClass;
                this.InstantiationType = serviceImplementationClass.GetCustomAttribute<ServiceProviderAttribute>()?.Type ?? ServiceInstantiationType.Singleton;
                this.Preferred = this.ServiceImplementer.GetCustomAttributes<PreferredServiceAttribute>().Select(o => o.ServiceType).ToArray();
                this.m_implementedServices = new HashSet<Type>(serviceImplementationClass.GetInterfaces().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o)));
            }

            /// <summary>
            /// Create from a singleton
            /// </summary>
            public ServiceInstanceInformation(Object singleton, DependencyServiceManager serviceManager) : this(serviceManager)
            {
                this.ServiceImplementer = singleton.GetType();
                this.InstantiationType = this.ServiceImplementer.GetCustomAttribute<ServiceProviderAttribute>()?.Type ?? ServiceInstantiationType.Singleton;
                this.m_singletonInstance = singleton;
                this.Preferred = this.ServiceImplementer.GetCustomAttributes<PreferredServiceAttribute>().Select(o => o.ServiceType).ToArray();
                this.m_implementedServices = new HashSet<Type>(this.ServiceImplementer.GetInterfaces().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o)));
            }

            /// <summary>
            /// Get the created instance otherwise null
            /// </summary>
            internal object GetCreatedInstance() => this.m_singletonInstance;

            /// <summary>
            /// Types that this is preferred for
            /// </summary>
            public Type[] Preferred { get; }

            /// <summary>
            /// Get the dependent services (singletons only)
            /// </summary>
            public Type[] InjectedServices => this.m_injectedServices;

            /// <summary>
            /// Get an instance of the object
            /// </summary>
            public object GetInstance()
            {
                // Is there already an object activator?
                lock (this.m_lockBox)
                {
                    if (this.m_singletonInstance != null)
                    {
                        return this.m_singletonInstance;
                    }
                    else if (this.InstantiationType == ServiceInstantiationType.Singleton)
                    {
                        this.m_singletonInstance = this.m_serviceManager.CreateInjectedInternal(this.ServiceImplementer, out var dependencies);
                        this.m_injectedServices = dependencies;
                        return this.m_singletonInstance;
                    }
                    else
                    {
                        return this.m_serviceManager.CreateInjected(this.ServiceImplementer);
                    }
                }
            }

            /// <summary>
            /// Dispose object
            /// </summary>
            public void Dispose()
            {
                if (this.m_singletonInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            /// <summary>
            /// Gets or sets the instantiation type
            /// </summary>
            public ServiceInstantiationType InstantiationType { get; }

            /// <summary>
            /// Gets the implemented services
            /// </summary>
            public IEnumerable<Type> ImplementedServices => this.m_implementedServices;

            /// <summary>
            /// Service implementer
            /// </summary>
            public Type ServiceImplementer { get; }

            /// <inheritdoc/>
            public override string ToString() => $"{this.InstantiationType} - {this.ServiceImplementer.FullName}";
        }

        /// <summary>
        /// Creates a new dependency service manager
        /// </summary>
        public DependencyServiceManager()
        {
            this.AddServiceProvider(this);
            this.AddServiceFactory(new DefaultServiceFactory(this));
        }

        // Disposed?
        private bool m_isDisposed = false;

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DependencyServiceManager));

        // Lock for the sync
        private object m_lock = new object();

        // Configuration
        private ApplicationServiceContextConfigurationSection m_configuration;

        // Service registrations
        private List<ServiceInstanceInformation> m_serviceRegistrations = new List<ServiceInstanceInformation>();

        // Services
        private ConcurrentDictionary<Type, ServiceInstanceInformation> m_cachedServices = new ConcurrentDictionary<Type, ServiceInstanceInformation>();

        /// <summary>
        /// Application is starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Application has started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Application is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Application has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// True if the service is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Dependency Injection Service Manager";

        /// <summary>
        /// Adds a service provider
        /// </summary>
        public void AddServiceProvider(Type serviceType)
        {
            lock (this.m_lock)
            {
                if (this.m_serviceRegistrations.Any(s => s.ServiceImplementer == serviceType))
                {
                    this.m_tracer.TraceWarning("Service {0} has already been registered...", serviceType);
                }
                else
                {
                    this.ValidateServiceSignature(serviceType);
                    var sii = new ServiceInstanceInformation(serviceType, this);
                    this.m_serviceRegistrations.Add(sii);

                    if (typeof(IServiceFactory).IsAssignableFrom(serviceType))
                    {
                        this.AddServiceFactory(sii.GetInstance() as IServiceFactory);
                    }
                    this.AddCacheServices(sii);
                }
                this.m_notConfiguredServices.Clear();
            }
        }

        /// <summary>
        /// Add service provider
        /// </summary>
        private void AddServiceProvider(ServiceInstanceInformation serviceInfo)
        {
            if (this.m_serviceRegistrations.Any(s => s.ServiceImplementer == serviceInfo.ServiceImplementer))
            {
                this.m_tracer.TraceWarning("Service {0} has already been registered...", serviceInfo.ServiceImplementer);
            }
            else
            {
                this.m_serviceRegistrations.Add(serviceInfo);
                if (typeof(IServiceFactory).IsAssignableFrom(serviceInfo.ServiceImplementer))
                {
                    this.AddServiceFactory(serviceInfo.GetInstance() as IServiceFactory);
                }

                this.AddCacheServices(serviceInfo);
            }
        }

        /// <summary>
        /// Adds a singleton
        /// </summary>
        public void AddServiceProvider(object serviceInstance)
        {
            lock (this.m_lock)
            {
                // Duplicate check 
                if (this.m_serviceRegistrations.Any(s => s.ServiceImplementer == serviceInstance.GetType()))
                {
                    this.m_tracer.TraceWarning("Service {0} has already been registered...", serviceInstance.GetType());
                }
                else
                {
                    if (serviceInstance is IConfigurationManager cmgr && this.m_configuration == null)
                    {
                        this.m_configuration = cmgr.GetSection<ApplicationServiceContextConfigurationSection>();
                    }

                    this.ValidateServiceSignature(serviceInstance.GetType());
                    var serviceInfo = new ServiceInstanceInformation(serviceInstance, this);
                    this.m_serviceRegistrations.Add(serviceInfo);
                    this.m_notConfiguredServices.Clear();

                    if (serviceInstance is IServiceFactory sf)
                    {
                        this.AddServiceFactory(sf);
                    }
                    this.AddCacheServices(serviceInfo);
                }

            }
        }

        /// <summary>
        /// Get all types
        /// </summary>
        public IEnumerable<Type> GetAllTypes() => AppDomain.CurrentDomain.GetAllTypes();

        /// <summary>
        /// Get service in a safe manner
        /// </summary>
        public object GetServiceSafe(Type serviceType)
        {
            try
            {
                return this.GetService(serviceType);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Get the specified service
        /// </summary>
        public object GetService(Type serviceType) => this.GetServiceInternal(serviceType);

        /// <summary>
        /// Get service with <paramref name="excludeImplementations"/>
        /// </summary>
        private object GetServiceInternal(Type serviceType, Type[] excludeImplementations = null)
        {
            ServiceInstanceInformation candidateService = null;
            if (this.m_cachedServices?.TryGetValue(serviceType, out candidateService) == false
                && !this.m_notConfiguredServices.Contains(serviceType)
                || excludeImplementations?.Any(s => candidateService?.Preferred?.Contains(s) == true) == true)
            {
                lock (this.m_lock)
                {
                    candidateService = this.m_serviceRegistrations.FirstOrDefault(s => (s.ImplementedServices.Contains(serviceType) || serviceType.IsAssignableFrom(s.ServiceImplementer)) && excludeImplementations?.Any(a => s.Preferred.Contains(a)) != true);
                    if (candidateService == null) // Attempt a load from configuration
                    {
                        var cServiceType = this.m_configuration.ServiceProviders.FirstOrDefault(s => s.Type != null && serviceType.IsAssignableFrom(s.Type));
                        if (cServiceType != null)
                        {
                            candidateService = new ServiceInstanceInformation(cServiceType.Type, this);
                            if (excludeImplementations?.Any(a => candidateService.Preferred.Contains(a)) != true)
                            {
                                this.m_serviceRegistrations.Add(candidateService);
                                this.AddServiceProvider(candidateService);
                            }
                            else
                            {
                                candidateService = null; // skip and move on
                            }
                        }
                        if (candidateService == null) // Attempt to call the service factories to create it
                        {
                            var created = false;
                            foreach (var factory in this.m_serviceFactories)
                            {
                                // Is the service factory already created?
                                created |= factory.TryCreateService(serviceType, out object serviceInstance);
                                if (created)
                                {
                                    candidateService = new ServiceInstanceInformation(serviceInstance, this);
                                    this.AddServiceProvider(candidateService);
                                    break;
                                }
                            }
                            if (!created)
                            {
                                this.m_notConfiguredServices.Add(serviceType);
                            }
                        }

                    }

                }
            }
            return candidateService?.GetInstance();
        }

        /// <summary>
        /// Add service <paramref name="candidateService"/> to cached services
        /// </summary>
        private void AddCacheServices(ServiceInstanceInformation candidateService)
        {

            // Is the candidate service a preferred service? If so remove the others
            foreach (var preferredService in candidateService.Preferred)
            {
                this.m_cachedServices.TryRemove(preferredService, out _);
            }

            foreach (var itm in candidateService.ImplementedServices)
            {
                this.m_cachedServices.TryAdd(itm, candidateService);
            }

        }

        /// <summary>
        /// Gets all service instances
        /// </summary>
        public IEnumerable<object> GetServices()
        {
            lock (this.m_lock)
            {
                return this.m_serviceRegistrations.ToArray().Where(o => o.InstantiationType == ServiceInstantiationType.Singleton).Select(o => o.GetInstance());
            }
        }

        /// <summary>
        /// Remove a service provider
        /// </summary>
        public void RemoveServiceProvider(Type serviceType)
        {
            var serviceProviders = this.m_serviceRegistrations.Where(sr => sr.ImplementedServices.Contains(serviceType) || serviceType.IsAssignableFrom(sr.ServiceImplementer)).ToArray();
            this.m_tracer.TraceVerbose("Removing {0}...", serviceType);
            // iterate and dispose :)
            foreach (var sp in serviceProviders)
            {
                this.m_tracer.TraceVerbose("{0} implements {1} so is being removed", sp.ServiceImplementer, serviceType);
                if (sp.InstantiationType == ServiceInstantiationType.Singleton)
                {
                    var singleton = sp.GetInstance();
                    if (singleton is IDaemonService daemon)
                    {
                        this.m_tracer.TraceVerbose("{0} implements IDaemon - Shutting down...", sp.ServiceImplementer);
                        daemon.Stop();
                    }
                }
                sp.Dispose();

                // Remove
                lock (this.m_lock)
                {
                    this.m_serviceRegistrations.Remove(sp);
                }

                foreach (var i in sp.ImplementedServices)
                {
                    if (this.m_cachedServices.TryGetValue(i, out ServiceInstanceInformation v) && v.ServiceImplementer == sp.ServiceImplementer)
                    {
                        this.m_cachedServices.TryRemove(i, out ServiceInstanceInformation _);
                    }


                    // Any dependency information for this with no other implementers
                    foreach (var itm in this.m_serviceRegistrations.Where(r => r.InjectedServices?.Contains(i) == true)) // this was injected into that so remove that
                    {
                        this.m_cachedServices.TryRemove(itm.ServiceImplementer, out _);
                    }
                }

            }
        }

        /// <summary>
        /// Dispose of this object
        /// </summary>
        public void Dispose()
        {
            if (this.m_isDisposed == true)
            {
                return;
            }

            this.m_isDisposed = true;
            if (this.m_serviceRegistrations != null)
            {
                foreach (var sp in this.m_serviceRegistrations.ToArray())
                {
                    if (sp.ServiceImplementer != typeof(DependencyServiceManager))
                    {
                        this.m_tracer.TraceVerbose("Disposing {0}...", sp.ServiceImplementer);
                        sp.Dispose();
                    }
                }
            }
            this.m_tracer.TraceInfo("Disposing of HTTP Thread Pool and Probes..");
            DiagnosticsProbeManager.Current.Dispose();
            RestServerThreadPool.Current.Dispose();
        }

        /// <summary>
        /// Start the process
        /// </summary>
        public bool Start()
        {
            if (!this.IsRunning)
            {
                Stopwatch startWatch = new Stopwatch();

                try
                {
                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.0f, UserMessages.STARTING_CONTEXT));
                    if (this.GetService<IConfigurationManager>() == null)
                    {
                        throw new InvalidOperationException("Cannot find configuration manager!");
                    }

                    if (this.m_configuration == null)
                    {
                        this.m_configuration = this.GetService<IConfigurationManager>().GetSection<ApplicationServiceContextConfigurationSection>();
                    }

                    // Add configured services
                    int i = 0;
                    foreach (var svc in this.m_configuration.ServiceProviders
                        .Select(s => new { serviceType = s, order = s.Type.GetCustomAttributes<PreferredServiceAttribute>().Count() + (s.Type.Implements(typeof(IServiceFactory)) ? 100 : 0) })
                        .OrderByDescending(s => s.order)
                        .Select(s => s.serviceType))
                    {

                        this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(((float)i++ / this.m_configuration.ServiceProviders.Count) * 0.3f, UserMessages.STARTING_CONTEXT));

                        if (svc.Type == null)
                        {
                            this.m_tracer.TraceWarning("Cannot find service {0}, skipping", svc.TypeXml);
                        }
                        else if (this.m_serviceRegistrations.Any(p => p.ServiceImplementer == svc.Type))
                        {
                            this.m_tracer.TraceWarning("Duplicate registration of type {0}, skipping", svc.TypeXml);
                        }
                        else
                        {
                            this.AddServiceProvider(svc.Type);
                        }
                    }

                    using (AuthenticationContext.EnterSystemContext())
                    {
                        this.Starting?.Invoke(this, EventArgs.Empty);

                        startWatch.Start();
                        this.m_tracer.TraceInfo("Loading singleton services");
                        i = 0;
                        var singletonServices = this.m_serviceRegistrations.ToArray().Where(o => o.InstantiationType == ServiceInstantiationType.Singleton).ToArray();
                        foreach (var svc in singletonServices)
                        {
                            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(((float)i++ / singletonServices.Length) * 0.3f + 0.3f, UserMessages.INITIALIZE_SINGLETONS));
                            this.m_tracer.TraceVerbose("Instantiating {0}...", svc.ServiceImplementer.FullName);
                            svc.GetInstance();
                        }

                        this.m_tracer.TraceInfo("Starting Daemon services");
                        var daemonServices = this.m_serviceRegistrations.ToArray().Where(o => o.ImplementedServices.Contains(typeof(IDaemonService))).Select(o => o.GetInstance() as IDaemonService).ToArray();
                        i = 0;
                        foreach (var dc in daemonServices)
                        {
                            if (dc == null)
                            {
                                continue;
                            }

                            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(((float)i++ / daemonServices.Length) * 0.3f + 0.6f, String.Format(UserMessages.START_DAEMON, dc.ServiceName)));

                            this.m_tracer.TraceInfo("Starting {0}...", dc.ServiceName);
                            if (dc != this && !dc.Start())
                            {
                                throw new Exception($"Service {dc} reported unsuccessful start");
                            }
                        }

                        this.Started?.Invoke(this, EventArgs.Empty);
                    }
                }
                finally
                {
                    startWatch.Stop();
                }
                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(1.0f, UserMessages.STARTING_CONTEXT));

                this.m_tracer.TraceInfo("Startup completed successfully in {0} ms...", startWatch.ElapsedMilliseconds);
                this.IsRunning = true;
            }
            return this.IsRunning;
        }

        /// <summary>
        /// Validates that the assembly is signed either by authenticode or via
        /// </summary>
        private void ValidateServiceSignature(Type type)
        {
            type.Assembly.ValidateCodeIsSigned(this.m_configuration?.AllowUnsignedAssemblies == true);
        }

        /// <summary>
        /// Stop this instance
        /// </summary>
        public bool Stop()
        {
            this.m_tracer.TraceInfo("Stopping dependency injection service...");

            this.Stopping?.Invoke(this, null);

            if (!this.IsRunning)
            {
                return true;
            }

            this.IsRunning = false;

            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.0f, UserMessages.STOPPING_CONTEXT));
            int i = 0;
            foreach (var svc in this.m_serviceRegistrations.Where(o => o.ServiceImplementer != typeof(DependencyServiceManager) && !o.ServiceImplementer.Implements(typeof(IApplicationServiceContext))).ToArray())
            {
                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs((float)i++ / this.m_serviceRegistrations.Count, UserMessages.STOPPING_CONTEXT));

                if (svc.InstantiationType == ServiceInstantiationType.Singleton && svc.GetCreatedInstance() is IDaemonService daemon)
                {
                    this.m_tracer.TraceInfo("Stopping {0}...", svc.ServiceImplementer.Name);
                    daemon.Stop();
                }
                this.m_tracer.TraceVerbose("Disposing service {0}...", svc.ServiceImplementer.Name);
                svc.Dispose();
            }

            this.Stopped?.Invoke(this, null);
            return true;
        }

        /// <summary>
        /// Create injected type
        /// </summary>
        public object CreateInjected(Type type) => this.CreateInjectedInternal(type, out _);

        /// <summary>
        /// Create injected type
        /// </summary>
        /// <param name="type">The type to be injected</param>
        /// <param name="injectedServices">The injected depencies</param>
        internal object CreateInjectedInternal(Type type, out Type[] injectedServices)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Cannot find type for dependency injection");
            }

            var preferredForServices = type.GetCustomAttributes<PreferredServiceAttribute>().Select(t => t.ServiceType).ToArray();

            // DI stack value
            this.m_dependencyInjectionStack.Value = this.m_dependencyInjectionStack.Value ?? new Stack<Type>();

            // Check for circular refs
            if (this.m_dependencyInjectionStack.Value.Contains(type))
            {
                throw new InvalidOperationException($"Circular dependencies detected - {String.Join(">", this.m_dependencyInjectionStack.Value.Reverse().Select(o => o.Name))}>{type.Name}>!");
            }
            this.m_dependencyInjectionStack.Value.Push(type);

            try
            {
                if (!this.m_activators.TryGetValue(type, out Func<Object> activator))
                {
                    // TODO: Check for circular dependencies
                    var constructors = type.GetConstructors();

                    // Is it a parameterless constructor?
                    var constructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
                    if (constructor != null)
                    {
                        activator = Expression.Lambda<Func<Object>>(Expression.New(constructor)).Compile();
                        injectedServices = Type.EmptyTypes;
                    }
                    else
                    {
                        // Get a constructor that we can fulfill
                        constructor = constructors.SingleOrDefault();
                        if (constructor == null)
                        {
                            throw new MissingMemberException($"Cannot find default constructor on {type}");
                        }

                        var parameterTypes = constructor.GetParameters().Select(p => new { Type = p.ParameterType, Required = !p.HasDefaultValue, Default = p.DefaultValue }).ToArray();
                        var parameterValues = new Expression[parameterTypes.Length];
                        injectedServices = new Type[parameterTypes.Length];

                        for (int i = 0; i < parameterValues.Length; i++)
                        {
                            var dependencyInfo = parameterTypes[i];
                            var dependentServiceType = injectedServices[i] = dependencyInfo.Type;
                            // Is the dependent service anything for which we are the preferred service? If so find another instance
                            object candidateService = this.GetServiceInternal(dependentServiceType, preferredForServices);
                            if (candidateService == null && dependencyInfo.Required)
                            {
                                throw new InvalidOperationException($"Service {type} relies on {dependencyInfo.Type} but no service of type {dependencyInfo.Type.Name} has been registered! Not Instantiated");
                            }
                            else
                            {
                                if (candidateService != null && preferredForServices.Any(s => s.IsAssignableFrom(candidateService.GetType()))) // Replace with a specific implementation
                                {
                                    dependentServiceType = candidateService.GetType();
                                }
                                var expr = Expression.Convert(Expression.Call(
                                    Expression.Constant(this),
                                    (MethodInfo)typeof(DependencyServiceManager).GetMethod(nameof(GetServiceSafe)),
                                    Expression.Constant(dependentServiceType)), dependencyInfo.Type);
                                //Expression<Func<object,dynamic>> expr = (_) => ApplicationServiceContext.Current.GetService<Object>();
                                parameterValues[i] = expr; // Expression.Convert(Expression.Constant(candidateService), dependentServiceType); //expr;
                            }
                        }

                        // Now we can create our activator
                        activator = Expression.Lambda<Func<Object>>(Expression.New(constructor, parameterValues.ToArray())).Compile();
                    }

                    this.m_activators.TryAdd(type, activator);
                }
                else
                {
                    injectedServices = activator.Method.GetParameters().Select(o => o.ParameterType).ToArray();
                }
                injectedServices = injectedServices.OfType<Type>().ToArray(); // filter out nulls
                return activator();
            }
            finally
            {
                this.m_dependencyInjectionStack.Value.Pop();
            }
        }

        /// <summary>
        /// Create injected instance
        /// </summary>
        public TObject CreateInjected<TObject>()
        {
            return (TObject)this.CreateInjected(typeof(TObject));
        }

        /// <summary>
        /// Create injected instances of all implementers of the specified <typeparamref name="TInterface"/>
        /// </summary>
        /// <typeparam name="TInterface">The type of interface to construct</typeparam>
        public IEnumerable<TInterface> CreateInjectedOfAll<TInterface>(Assembly fromAssembly = null)
        {
            var interfacetype = typeof(TInterface);

            if (fromAssembly == null)
            {
                return this.GetAllTypes()
                    .Where(t =>
                    {
                        try
                        {
                            return interfacetype.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsPublic;
                        }
                        catch { return false; }
                    })
                    .Select(t =>
                    {
                        try
                        {
                            return this.CreateInjected(t);
                        }
                        catch (Exception e)
                        {
                            this.m_tracer.TraceWarning($"CreateInjectedOfAll<> cannot create {t} due to {e.Message}");
                            return null;
                        }
                    })
                    .OfType<TInterface>();
            }
            else
            {
                return fromAssembly.GetExportedTypesSafe().Where(t => interfacetype.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                        .Select(t => this.CreateInjected(t))
                        .OfType<TInterface>();

            }
        }

        /// <summary>
        /// Add a service factory
        /// </summary>
        public void AddServiceFactory(IServiceFactory serviceFactory)
        {
            lock (this.m_lock)
            {
                if (!this.m_serviceFactories.Any(t => t.GetType() == serviceFactory.GetType()))
                {
                    this.m_serviceFactories.Add(serviceFactory);
                }
            }
        }

        /// <summary>
        /// Create all instances of <typeparamref name="T"/>
        /// </summary>
        public IEnumerable<T> CreateAll<T>(params object[] parms)
        {
            var ttype = typeof(T);

            return this.GetAllTypes()
                .Where(t =>
                {
                    try
                    {
                        return ttype.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsPublic && t.GetConstructor(parms.Select(o => o.GetType()).ToArray()) != null;
                    }
                    catch { return false; }
                    }).Select(t =>
                    {
                        try
                        {
                            return Activator.CreateInstance(t, parms);
                        }
                        catch
                        {
                            return Activator.CreateInstance(t);
                        }
                    })
                .OfType<T>();
        }

        /// <inheritdoc/>
        public void NotifyStartupProgress(float startupProgress, string startupChangeText)
        {
            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(startupProgress, startupChangeText));
        }
    }
}