using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a service manager and provider that supports DI
    /// </summary>
    /// <remarks>You must have an IConfigurationManager instance registered in order to use this service</remarks>
    public class DependencyServiceManager : IServiceManager, IServiceProvider, IDaemonService, IDisposable
    {

        /// <summary>
        /// Gets the service instance information
        /// </summary>
        private class ServiceInstanceInformation : IDisposable
        {

            // Tracer
            private Tracer m_tracer = Tracer.GetTracer(typeof(ServiceInstanceInformation));

            // The delegate which can construct the object
            private Func<Object> m_activator = null;

            // Singleton instance
            private object m_singletonInstance = null;

            // Lock object
            private object m_lockBox = new object();

            /// <summary>
            /// Gets the service instance information
            /// </summary>
            public ServiceInstanceInformation(Type serviceImplementationClass)
            {
                this.ServiceImplementer = serviceImplementationClass;
                this.InstantiationType = serviceImplementationClass.GetCustomAttribute<ServiceProviderAttribute>()?.Type ?? ServiceInstantiationType.Singleton;
                this.ImplementedServices = serviceImplementationClass.GetInterfaces().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o)).ToArray();
            }

            /// <summary>
            /// Create from a singleton
            /// </summary>
            public ServiceInstanceInformation(Object singleton)
            {
                this.ServiceImplementer = singleton.GetType();
                this.InstantiationType = this.ServiceImplementer.GetCustomAttribute<ServiceProviderAttribute>()?.Type ?? ServiceInstantiationType.Singleton;
                this.m_singletonInstance = singleton;
                this.ImplementedServices = this.ServiceImplementer.GetInterfaces().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o)).ToArray();

            }

            /// <summary>
            /// Get an instance of the object
            /// </summary>
            public object GetInstance()
            {
                // Is there already an object activator?
                lock (this.m_lockBox)
                    if (this.m_singletonInstance != null)
                        return this.m_singletonInstance;
                    else if (this.m_activator != null)
                    {
                        return this.m_activator();
                    }
                    else
                    {
                        // TODO: Check for circular dependencies
                        var constructors = this.ServiceImplementer.GetConstructors();

                        // Is it a parameterless constructor?
                        var constructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
                        if (constructor != null)
                            this.m_activator = Expression.Lambda<Func<Object>>(Expression.New(constructor)).Compile();
                        else
                        {
                            // Get a constructor that we can fulfill
                            constructor = constructors.SingleOrDefault();
                            if (constructor == null)
                                throw new MissingMemberException($"Cannot find default constructor on {this.ServiceImplementer}");

                            var parameterTypes = constructor.GetParameters().Select(p => new { Type = p.ParameterType, Required = !p.HasDefaultValue, Default = p.DefaultValue }).ToArray();
                            var parameterValues = new Expression[parameterTypes.Length];
                            for (int i = 0; i < parameterValues.Length; i++)
                            {
                                var dependencyInfo = parameterTypes[i];
                                var candidateService = ApplicationServiceContext.Current.GetService(dependencyInfo.Type); // We do this because we don't want GetService<> to initialize the type;
                                if (candidateService == null && dependencyInfo.Required)
                                {
                                    this.m_tracer.TraceWarning($"Service {this.ServiceImplementer} relies on {dependencyInfo.Type} but no service of type {dependencyInfo.Type.Name} has been registered! Not Instantiated");
                                    return null;
                                }
                                else
                                    parameterValues[i] = Expression.Convert(Expression.Constant(candidateService ?? dependencyInfo.Default), dependencyInfo.Type);
                            }

                            // Now we can create our activator
                            this.m_activator = Expression.Lambda<Func<Object>>(Expression.New(constructor, parameterValues.ToArray())).Compile();
                        }

                        if (this.InstantiationType == ServiceInstantiationType.Singleton)
                        {
                            this.m_singletonInstance = this.m_activator();
                            return this.m_singletonInstance;
                        }
                        else
                            return this.m_activator();

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
                if (this.m_serviceRegistrations.Any(s => s.ServiceImplementer == serviceType))
                    this.m_tracer.TraceWarning("Service {0} has already been registered...", serviceType);
                else 
                    this.m_serviceRegistrations.Add(new ServiceInstanceInformation(serviceType));
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
                this.m_serviceRegistrations.Add(new ServiceInstanceInformation(serviceInstance));
            }
        }

        /// <summary>
        /// Get all types
        /// </summary>
        public IEnumerable<Type> GetAllTypes()
        {
            // HACK: The weird TRY/CATCH in select many is to prevent mono from throwning a fit
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => { try { return a.ExportedTypes; } catch { return new List<Type>(); } });
        }

        /// <summary>
        /// Get the specified service
        /// </summary>
        public object GetService(Type serviceType)
        {
            ServiceInstanceInformation candidateService = null;
            if (this.m_cachedServices?.TryGetValue(serviceType, out candidateService) == false)
            {
                lock (this.m_lock)
                {
                    candidateService = this.m_serviceRegistrations.FirstOrDefault(s => s.ImplementedServices.Contains(serviceType) || serviceType.IsAssignableFrom(s.ServiceImplementer));
                    if (candidateService == null) // Attempt a load from configuration
                    {
                        var cServiceType = this.m_configuration.ServiceProviders.SingleOrDefault(s => s.Type != null && serviceType.IsAssignableFrom(s.Type));
                        if (cServiceType != null)
                        {
                            candidateService = new ServiceInstanceInformation(cServiceType.Type);
                            this.m_serviceRegistrations.Add(candidateService);
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
                    this.Starting?.Invoke(this, EventArgs.Empty);

                    startWatch.Start();

                    if (this.GetService<IConfigurationManager>() == null)
                        throw new InvalidOperationException("Cannot find configuration manager!");
                    if (this.m_configuration == null)
                        this.m_configuration = this.GetService<IConfigurationManager>().GetSection<ApplicationServiceContextConfigurationSection>();

                    // Add configured services
                    foreach (var svc in this.m_configuration.ServiceProviders)
                        if (svc.Type == null)
                            this.m_tracer.TraceWarning("Cannot find service {0}, skipping", svc.TypeXml);
                        else if (this.m_serviceRegistrations.Any(p => p.ServiceImplementer == svc.Type))
                            this.m_tracer.TraceWarning("Duplicate registration of type {0}, skipping", svc.TypeXml);
                        else
                        {
                            var svci = new ServiceInstanceInformation(svc.Type);

                            this.m_serviceRegistrations.Add(svci);
                            foreach (var iface in svci.ImplementedServices)
                                this.m_cachedServices.TryAdd(iface, svci);
                        }

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
        /// Stop this instance
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, null);

            if (!this.IsRunning) return true ;

            this.IsRunning = false;

            foreach (var svc in this.m_serviceRegistrations.Where(o=>o.ServiceImplementer != typeof(DependencyServiceManager)).ToArray())
            {
                if (svc.InstantiationType == ServiceInstantiationType.Singleton && svc.GetInstance() is IDaemonService daemon)
                {
                    this.m_tracer.TraceInfo("Stopping daemon service {0}...", svc.ServiceImplementer.Name);
                    daemon.Stop();
                }
            }

            this.Stopped?.Invoke(this, null);
            return true;
        }
    }
}
