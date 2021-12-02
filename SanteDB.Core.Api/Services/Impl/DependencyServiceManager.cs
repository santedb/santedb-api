/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
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
    /// Represents a service manager and provider that supports DI
    /// </summary>
    /// <remarks>You must have an IConfigurationManager instance registered in order to use this service</remarks>
    public class DependencyServiceManager : IServiceManager, IServiceProvider, IDaemonService, IDisposable
    {
        // DI Stack
        private ThreadLocal<Stack<Type>> m_dependencyInjectionStack = new ThreadLocal<Stack<Type>>();

        // Activators
        private ConcurrentDictionary<Type, Func<Object>> m_activators = new ConcurrentDictionary<Type, Func<object>>();

        // Not configured services
        private HashSet<Type> m_notConfiguredServices = new HashSet<Type>();

        // Service factories
        private HashSet<IServiceFactory> m_serviceFactories = new HashSet<IServiceFactory>();

        // Verified assemblies
        private HashSet<String> m_verifiedAssemblies = new HashSet<string>();

        /// <summary>
        /// Gets the service instance information
        /// </summary>
        private class ServiceInstanceInformation : IDisposable
        {
            // Tracer
            private Tracer m_tracer = Tracer.GetTracer(typeof(ServiceInstanceInformation));

            // Singleton instance
            private object m_singletonInstance = null;

            // Lock object
            private object m_lockBox = new object();

            // Service manager
            private DependencyServiceManager m_serviceManager;

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
                this.ImplementedServices = serviceImplementationClass.GetInterfaces().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o)).ToArray();
            }

            /// <summary>
            /// Create from a singleton
            /// </summary>
            public ServiceInstanceInformation(Object singleton, DependencyServiceManager serviceManager) : this(serviceManager)
            {
                this.ServiceImplementer = singleton.GetType();
                this.InstantiationType = this.ServiceImplementer.GetCustomAttribute<ServiceProviderAttribute>()?.Type ?? ServiceInstantiationType.Singleton;
                this.m_singletonInstance = singleton;
                this.ImplementedServices = this.ServiceImplementer.GetInterfaces().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o)).ToArray();
            }

            /// <summary>
            /// Get the created instance otherwise null
            /// </summary>
            internal object GetCreatedInstance() => this.m_singletonInstance;

            /// <summary>
            /// Get an instance of the object
            /// </summary>
            public object GetInstance()
            {
                // Is there already an object activator?
                lock (this.m_lockBox)
                    if (this.m_singletonInstance != null)
                        return this.m_singletonInstance;
                    else if (this.InstantiationType == ServiceInstantiationType.Singleton)
                    {
                        this.m_singletonInstance = this.m_serviceManager.CreateInjected(this.ServiceImplementer);
                        return this.m_singletonInstance;
                    }
                    else
                    {
                        return this.m_serviceManager.CreateInjected(this.ServiceImplementer);
                    }
            }

            /// <summary>
            /// Dispose object
            /// </summary>
            public void Dispose()
            {
                if (this.m_singletonInstance is IDisposable disposable)
                    disposable.Dispose();
            }

            /// <summary>
            /// Gets or sets the instantiation type
            /// </summary>
            public ServiceInstantiationType InstantiationType { get; }

            /// <summary>
            /// Gets the implemented services
            /// </summary>
            public IEnumerable<Type> ImplementedServices { get; }

            /// <summary>
            /// Service implementer
            /// </summary>
            public Type ServiceImplementer { get; }
        }

        /// <summary>
        /// Creates a new dependency service manager
        /// </summary>
        public DependencyServiceManager()
        {
            this.AddServiceProvider(this);
        }

        // Disposed?
        private bool m_isDisposed = false;

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DependencyServiceManager));

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
                    this.m_tracer.TraceWarning("Service {0} has already been registered...", serviceType);
                else
                {
                    this.ValidateServiceSignature(serviceType);
                    var sii = new ServiceInstanceInformation(serviceType, this);
                    this.m_serviceRegistrations.Add(sii);

                    if (typeof(IServiceFactory).IsAssignableFrom(serviceType))
                    {
                        this.AddServiceFactory(sii.GetInstance() as IServiceFactory);
                    }
                }
                this.m_notConfiguredServices.Clear();
            }
        }

        /// <summary>
        /// Adds a singleton
        /// </summary>
        public void AddServiceProvider(object serviceInstance)
        {
            lock (this.m_lock)
            {
                if (serviceInstance is IConfigurationManager cmgr && this.m_configuration == null)
                    this.m_configuration = cmgr.GetSection<ApplicationServiceContextConfigurationSection>();
                this.ValidateServiceSignature(serviceInstance.GetType());
                this.m_serviceRegistrations.Add(new ServiceInstanceInformation(serviceInstance, this));
                this.m_notConfiguredServices.Clear();

                if (serviceInstance is IServiceFactory sf)
                {
                    this.AddServiceFactory(sf);
                }
            }
        }

        /// <summary>
        /// Get all types
        /// </summary>
        public IEnumerable<Type> GetAllTypes() => AppDomain.CurrentDomain.GetAllTypes();

        /// <summary>
        /// Get the specified service
        /// </summary>
        public object GetService(Type serviceType)
        {
            ServiceInstanceInformation candidateService = null;
            if (this.m_cachedServices?.TryGetValue(serviceType, out candidateService) == false && !this.m_notConfiguredServices.Contains(serviceType))
            {
                lock (this.m_lock)
                {
                    candidateService = this.m_serviceRegistrations.FirstOrDefault(s => s.ImplementedServices.Contains(serviceType) || serviceType.IsAssignableFrom(s.ServiceImplementer));
                    if (candidateService == null) // Attempt a load from configuration
                    {
                        var cServiceType = this.m_configuration.ServiceProviders.FirstOrDefault(s => s.Type != null && serviceType.IsAssignableFrom(s.Type));
                        if (cServiceType != null)
                        {
                            candidateService = new ServiceInstanceInformation(cServiceType.Type, this);
                            this.m_serviceRegistrations.Add(candidateService);
                        }
                        else // Attempt to call the service factories to create it
                        {
                            var created = false;
                            foreach (var factory in this.m_serviceFactories)
                            {
                                // Is the service factory already created?
                                created |= factory.TryCreateService(serviceType, out object serviceInstance);
                                if (created)
                                {
                                    candidateService = new ServiceInstanceInformation(serviceInstance, this);
                                    this.m_cachedServices.TryAdd(serviceType, candidateService);
                                    break;
                                }
                            }
                            if (!created)
                            {
                                this.m_notConfiguredServices.Add(serviceType);
                            }
                        }
                    }
                    if (candidateService != null)
                        this.m_cachedServices.TryAdd(serviceType, candidateService);
                }
            }
            return candidateService?.GetInstance();
        }

        /// <summary>
        /// Gets all service instances
        /// </summary>
        public IEnumerable<object> GetServices()
        {
            lock (this.m_lock)
                return this.m_serviceRegistrations.Where(o => o.InstantiationType == ServiceInstantiationType.Singleton).Select(o => o.GetInstance());
        }

        /// <summary>
        /// Remove a service provider
        /// </summary>
        public void RemoveServiceProvider(Type serviceType)
        {
            var serviceProviders = this.m_serviceRegistrations.Where(sr => sr.ImplementedServices.Contains(serviceType) || serviceType.IsAssignableFrom(sr.ServiceImplementer)).ToArray();
            this.m_tracer.TraceInfo("Removing {0}...", serviceType);
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
                    this.m_serviceRegistrations.Remove(sp);
                foreach (var i in sp.ImplementedServices)
                    if (this.m_cachedServices.TryGetValue(i, out ServiceInstanceInformation v) && v.ServiceImplementer == sp.ServiceImplementer)
                        this.m_cachedServices.TryRemove(i, out ServiceInstanceInformation _);
            }
        }

        /// <summary>
        /// Dispose of this object
        /// </summary>
        public void Dispose()
        {
            if (this.m_isDisposed == true) return;
            this.m_isDisposed = true;
            if (this.m_serviceRegistrations != null)
                foreach (var sp in this.m_serviceRegistrations)
                    if (sp.ServiceImplementer != typeof(DependencyServiceManager))
                    {
                        this.m_tracer.TraceInfo("Disposing {0}...", sp.ServiceImplementer);
                        sp.Dispose();
                    }
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
                    if (this.GetService<IConfigurationManager>() == null)
                        throw new InvalidOperationException("Cannot find configuration manager!");
                    if (this.m_configuration == null)
                        this.m_configuration = this.GetService<IConfigurationManager>().GetSection<ApplicationServiceContextConfigurationSection>();

                    // Add configured services
                    foreach (var svc in this.m_configuration.ServiceProviders)
                    {
                        if (svc.Type == null)
                            this.m_tracer.TraceWarning("Cannot find service {0}, skipping", svc.TypeXml);
                        else if (this.m_serviceRegistrations.Any(p => p.ServiceImplementer == svc.Type))
                            this.m_tracer.TraceWarning("Duplicate registration of type {0}, skipping", svc.TypeXml);
                        else
                        {
                            if (svc.Type == null)
                                this.m_tracer.TraceWarning("Cannot find service {0}, skipping", svc.TypeXml);
                            else if (this.m_serviceRegistrations.Any(p => p.ServiceImplementer == svc.Type))
                                this.m_tracer.TraceWarning("Duplicate registration of type {0}, skipping", svc.TypeXml);
                            else
                            {
                                this.AddServiceProvider(svc.Type);
                            }
                        }
                    }

                    using (AuthenticationContext.EnterSystemContext())
                    {
                        this.Starting?.Invoke(this, EventArgs.Empty);

                        startWatch.Start();
                        this.m_tracer.TraceInfo("Loading singleton services");
                        foreach (var svc in this.m_serviceRegistrations.ToArray().Where(o => o.InstantiationType == ServiceInstantiationType.Singleton))
                        {
                            this.m_tracer.TraceInfo("Instantiating {0}...", svc.ServiceImplementer.FullName);
                            svc.GetInstance();
                        }

                        this.m_tracer.TraceInfo("Starting Daemon services");
                        foreach (var dc in this.m_serviceRegistrations.ToArray().Where(o => o.ImplementedServices.Contains(typeof(IDaemonService))).Select(o => o.GetInstance() as IDaemonService))
                        {
                            if (dc == null) continue;
                            this.m_tracer.TraceInfo("Starting daemon {0}...", dc.ServiceName);
                            if (dc != this && !dc.Start())
                                throw new Exception($"Service {dc} reported unsuccessful start");
                        }

                        if (this.Started != null)
                            this.Started(this, null);
                    }
                }
                finally
                {
                    startWatch.Stop();
                }
                this.m_tracer.TraceInfo("Startup completed successfully in {0} ms...", startWatch.ElapsedMilliseconds);
                this.Started?.Invoke(this, EventArgs.Empty);
                this.IsRunning = true;
            }
            return this.IsRunning;
        }

        /// <summary>
        /// Validates that the assembly is signed either by authenticode or via
        /// </summary>
        private void ValidateServiceSignature(Type type)
        {
            bool valid = false;
            var asmFile = type.Assembly.Location;
            if (String.IsNullOrEmpty(asmFile))
            {
                this.m_tracer.TraceWarning("Cannot verify {0} - no assembly location found", asmFile);
            }
            else if (!this.m_configuration?.AllowUnsignedAssemblies == true)
            {
                // Verified assembly?
                if (!this.m_verifiedAssemblies.Contains(asmFile))
                {
                    try
                    {
                        var extraCerts = new X509Certificate2Collection();
                        extraCerts.Import(asmFile);

                        var certificate = new X509Certificate2(X509Certificate2.CreateFromSignedFile(asmFile));
                        this.m_tracer.TraceInfo("Validating {0} published by {1}", asmFile, certificate.Subject);
                        valid = certificate.IsTrustedIntern(extraCerts, out IEnumerable<X509ChainStatus> chainStatus);
                        if (!valid)
                        {
                            throw new SecurityException($"File {asmFile} published by {certificate.Subject} is not trusted in this environment ({String.Join(",", chainStatus.Select(o => $"{o.Status}:{o.StatusInformation}"))})");
                        }
                    }
                    catch (Exception e)
                    {
#if !DEBUG
                        throw new SecurityException($"Could not load digital signature information for {asmFile}", e);
#else
                        this.m_tracer.TraceWarning("Could not verify {0} due to error {1}", asmFile, e.Message);
                        valid = false;
#endif
                    }
                }
                else
                {
                    valid = true;
                }

                if (!valid)
                {
#if !DEBUG
                    throw new SecurityException($"Service {type} in assembly {asmFile} is not signed - or its signature could not be validated! Plugin may be tampered!");
#else
                    this.m_verifiedAssemblies.Add(asmFile);
                    this.m_tracer.TraceWarning("!!!!!!!!! ALERT !!!!!!! {0} in {1} is not signed - in a release version of SanteDB this will cause the host to not load this service!", type, asmFile);
#endif
                }
                else
                {
                    this.m_tracer.TraceVerbose("{0} was validated as trusted code", asmFile);
                    this.m_verifiedAssemblies.Add(asmFile);
                }
            }
        }

        /// <summary>
        /// Stop this instance
        /// </summary>
        public bool Stop()
        {
            this.m_tracer.TraceInfo("Stopping dependency injection service...");

            this.Stopping?.Invoke(this, null);

            if (!this.IsRunning) return true;

            this.IsRunning = false;

            foreach (var svc in this.m_serviceRegistrations.Where(o => o.ServiceImplementer != typeof(DependencyServiceManager)).ToArray())
            {
                if (svc.InstantiationType == ServiceInstantiationType.Singleton && svc.GetCreatedInstance() is IDaemonService daemon)
                {
                    this.m_tracer.TraceInfo("Stopping daemon service {0}...", svc.ServiceImplementer.Name);
                    daemon.Stop();
                }
                if (svc is IDisposable dsp)
                {
                    this.m_tracer.TraceInfo("Disposing service {0}...", svc.ServiceImplementer.Name);
                    dsp.Dispose();
                }
            }

            this.Stopped?.Invoke(this, null);
            return true;
        }

        /// <summary>
        /// Create injected type
        /// </summary>
        public object CreateInjected(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Cannot find type for dependency injection");
            }

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
                        activator = Expression.Lambda<Func<Object>>(Expression.New(constructor)).Compile();
                    else
                    {
                        // Get a constructor that we can fulfill
                        constructor = constructors.SingleOrDefault();
                        if (constructor == null)
                            throw new MissingMemberException($"Cannot find default constructor on {type}");

                        var parameterTypes = constructor.GetParameters().Select(p => new { Type = p.ParameterType, Required = !p.HasDefaultValue, Default = p.DefaultValue }).ToArray();
                        var parameterValues = new Expression[parameterTypes.Length];
                        for (int i = 0; i < parameterValues.Length; i++)
                        {
                            var dependencyInfo = parameterTypes[i];
                            var candidateService = ApplicationServiceContext.Current.GetService(dependencyInfo.Type); // We do this because we don't want GetService<> to initialize the type;
                            if (candidateService == null && dependencyInfo.Required)
                            {
                                throw new InvalidOperationException($"Service {type} relies on {dependencyInfo.Type} but no service of type {dependencyInfo.Type.Name} has been registered! Not Instantiated");
                            }
                            else
                            {
                                var expr = Expression.Convert(Expression.Call(
                                    Expression.MakeMemberAccess(null, typeof(ApplicationServiceContext).GetProperty(nameof(ApplicationServiceContext.Current))),
                                    (MethodInfo)typeof(IServiceProvider).GetMethod(nameof(GetService)),
                                    Expression.Constant(dependencyInfo.Type)), dependencyInfo.Type);
                                //Expression<Func<object,dynamic>> expr = (_) => ApplicationServiceContext.Current.GetService<Object>();
                                parameterValues[i] = expr;
                            }
                        }

                        // Now we can create our activator
                        activator = Expression.Lambda<Func<Object>>(Expression.New(constructor, parameterValues.ToArray())).Compile();
                    }

                    this.m_activators.TryAdd(type, activator);
                }
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
            if (fromAssembly == null)
            {
                return this.GetAllTypes()
                    .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
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
                return fromAssembly.ExportedTypes
                    .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
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
    }
}