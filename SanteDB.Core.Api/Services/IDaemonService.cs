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

using System;
using System.ComponentModel;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Defines a service which follows the daemon service pattern (<see href="https://help.santesuite.org/developers/server-plugins/implementing-.net-features/daemon-services"/>)
    /// </summary>
    /// <remarks>
    /// <para>In SanteDB (and OpenIZ) a daemon service is an actively executed service which is started on application host start and torn down/stopped on application
    /// context shutdown (or when initiated by a user). </para>
    /// <para>The <see cref="Start()"/> method is invoked on startup. It is expected that implementer of this class
    /// will raise the <see cref="Starting"/> event to signal to other services in the application context that this particular service is starting. Once the initialization process
    /// is complete, the implementation should call the <see cref="Started"/> event to signal this service has completed its necessary start, before returning true from the start method.
    /// This behavior allows chaining of dependent services together (i.e. don't start until after start of another service)</para>
    /// <para>On service teardown the <see cref="Stop()"/> method is called, again it is expected that implementers will raise <see cref="Stopping"/> and then <see cref="Stopped"/></para>
    /// <para>If the daemon also implements the .NET <see cref="IDisposable"/> interface, then the Dispose method is called after service shutdown.</para>
    /// </remarks>
    /// <example>
    /// <code language="cs" title="Implementing a Daemon Service">
    /// <![CDATA[
    /// public class HelloWorldDaemon : IDaemonService {
    ///
    ///     public event EventHandler Starting;
    ///
    ///     public event EventHandler Started;
    ///
    ///     public event EventHandler Stopping;
    ///
    ///     public event EventHandler Stopped;
    ///
    ///     public bool Start() {
    ///         this.Starting?.Invoke(this, EventArgs.Empty);
    ///         Console.WriteLine("Hello World!");
    ///         this.Started?.Invoke(this, EventArgs.Empty);
    ///         return true;
    ///     }
    ///
    ///     public bool Stop() {
    ///         this.Stopping?.Invoke(this, EventArgs.Empty);
    ///         Console.WriteLine("Goodbye World!");
    ///         this.Stopped?.Invoke(this, EventArgs.Empty);
    ///         return true;
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [Description("Daemon Service")]
    public interface IDaemonService : IServiceImplementation
    {
        /// <summary>
        /// Indicates the caller wishes to start the daemon service lifecycle
        /// </summary>
        /// <remarks><para>Implementers of this method should ensure that they perform any necessary startup tasks in this method. It is expected that implementers
        /// will raise the <see cref="Starting"/> and <see cref="Started"/> events from this method and will return TRUE if the service startup was successful. If
        /// startup is not successful, implementers will return FALSE at minimum (to place the host context in maintenance mode) or throw an exception (to terminate
        /// the host process)</para></remarks>
        /// <returns>True if service startup was successful, false if the daemon could not be started and should interrupt startup</returns>
        bool Start();

        /// <summary>
        /// Indicates the caller wishes to stop the daemon service
        /// </summary>
        /// <remarks><para>Implementers of this method should ensure that they perform any necessary tearing down of the service. It is good practice to
        /// release any connections, dispose of any unmanaged objects, etc. The implementer should raise the <see cref="Stopping"/> event followed by
        /// <see cref="Stopped"/> and return true if the stop was successful</para></remarks>
        /// <returns>True if the service was stopped successfully</returns>
        bool Stop();

        /// <summary>
        /// Indicates whether the daemon service is running
        /// </summary>
        /// <remarks>Implementers should evaluate whether the service is in a running state (i.e. are the connections still valid, etc.) and return
        /// true if the service is deemed to be in a running and available state</remarks>
        bool IsRunning { get; }

        /// <summary>
        /// Fired when the daemon service has commenced start but has not yet finished
        /// </summary>
        event EventHandler Starting;

        /// <summary>
        /// Fired when the daemon service has completed it start procedure.
        /// </summary>
        event EventHandler Started;

        /// <summary>
        /// Fired when the daemon service has commenced stop but has not yet been fully shut down.
        /// </summary>
        event EventHandler Stopping;

        /// <summary>
        /// Fired when the daemon has completed its stop procedure
        /// </summary>
        event EventHandler Stopped;
    }
}