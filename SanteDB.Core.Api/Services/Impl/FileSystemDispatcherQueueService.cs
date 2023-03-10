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
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// A persistent queue service that uses the file system (use when there's no other infrastructure)
    /// </summary>
    [DisplayName("File System Dispatcher"), Description("Persistent queue service using a file system directory for storage")]
    public class FileSystemDispatcherQueueService : IDispatcherQueueManagerService, IDisposable
    {

        // Ticks for system time
        private long m_ctr = DateTime.Now.Ticks;

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "File System Message Queue";

        // Disposed already?
        private volatile bool m_disposed = false;

        /// <summary>
        /// Queue entry
        /// </summary>
        [XmlType(nameof(QueueEntry), Namespace = "http://santedb.org/fsqueue")]
        [XmlRoot(nameof(QueueEntry), Namespace = "http://santedb.org/fsqueue")]
        public class QueueEntry
        {

            /// <summary>
            /// True if compressed
            /// </summary>
            [XmlAttribute("compressed")]
            public bool Compressed { get; set; }

            /// <summary>
            /// Data contained
            /// </summary>
            [XmlAttribute("type")]
            public String Type { get; set; }

            /// <summary>
            /// Creation time
            /// </summary>
            [XmlAttribute("creationTime")]
            public DateTime CreationTime { get; set; }

            /// <summary>
            /// Gets the data
            /// </summary>
            [XmlText]
            public byte[] XmlData { get; set; }

            /// <summary>
            /// Save the data to a stream
            /// </summary>
            public static QueueEntry Create(Object data)
            {
                XmlSerializer xsz = XmlModelSerializerFactory.Current.CreateSerializer(data.GetType());
                using (var ms = new MemoryStream())
                {
                    using (var df = new DeflateStream(ms, CompressionMode.Compress, true))
                    {
                        xsz.Serialize(df, data);
                    }
                    return new QueueEntry()
                    {
                        Compressed = true,
                        Type = data.GetType().AssemblyQualifiedName,
                        XmlData = ms.ToArray(),
                        CreationTime = DateTime.Now
                    };
                }
            }

            /// <summary>
            /// To object data
            /// </summary>
            public object ToObject()
            {
                XmlSerializer xsz = XmlModelSerializerFactory.Current.CreateSerializer(System.Type.GetType(this.Type));
                using (var ms = new MemoryStream(this.XmlData))
                {
                    if (this.Compressed)
                    {
                        using (var df = new DeflateStream(ms, CompressionMode.Decompress, false))
                        {
                            return xsz.Deserialize(df);
                        }
                    }
                    return xsz.Deserialize(ms);
                }
            }

            /// <summary>
            /// Load from stream
            /// </summary>
            public static QueueEntry Load(Stream str)
            {
                var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(QueueEntry));
                return xsz.Deserialize(str) as QueueEntry;
            }

            /// <summary>
            /// Save the queue entry on the stream
            /// </summary>
            public void Save(Stream str)
            {
                var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(QueueEntry));
                xsz.Serialize(str, this);
            }
        }

        private readonly ISymmetricCryptographicProvider m_symmetricCrypto;

        // Queue root directory
        private FileSystemDispatcherQueueConfigurationSection m_configuration;

        // Watchers
        private ConcurrentDictionary<String, List<DispatcherQueueCallback>> m_watchers = new ConcurrentDictionary<string, List<DispatcherQueueCallback>>();

        // Notification queue
        private ConcurrentQueue<DispatcherMessageEnqueuedInfo> m_notificationQueue = new ConcurrentQueue<DispatcherMessageEnqueuedInfo>();

        // Reset event
        private ManualResetEventSlim m_resetEvent = new ManualResetEventSlim(false);

        private CancellationTokenSource m_ListenThreadCancellationTokenSource;

        /// <summary>
        /// Queue file
        /// </summary>
        private Tracer m_tracer = Tracer.GetTracer(typeof(FileSystemDispatcherQueueService));

        // Pep service
        private readonly IPolicyEnforcementService m_pepService;

        // Listener thread
        private Thread m_listenerThread = null;

        /// <summary>
        /// Initializes the file system queue
        /// </summary>
        public FileSystemDispatcherQueueService(IConfigurationManager configurationManager, IPolicyEnforcementService pepService, ISymmetricCryptographicProvider symmetricCryptographicProvider)
        {
            this.m_symmetricCrypto = symmetricCryptographicProvider;
            this.m_configuration = configurationManager.GetSection<FileSystemDispatcherQueueConfigurationSection>() ??
                new FileSystemDispatcherQueueConfigurationSection() { QueuePath = "queue" };
            if (!Directory.Exists(this.m_configuration.QueuePath))
            {
                Directory.CreateDirectory(this.m_configuration.QueuePath);
            }

            this.m_pepService = pepService;

            this.m_ListenThreadCancellationTokenSource = new CancellationTokenSource();

            // Listener thread
            this.m_listenerThread = new Thread((object state) =>
            {
                CancellationToken token;

                if (state is CancellationToken cts)
                {
                    token = cts;
                }
                else
                {
                    token = CancellationToken.None;
                }

                while (!(this.m_disposed || token.IsCancellationRequested))
                {
                    try
                    {
                        this.m_resetEvent.Wait(1000, token);
                    }
                    catch (OperationCanceledException) { }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    while (this.m_notificationQueue.TryDequeue(out var result) && !String.IsNullOrEmpty(this.GetQueueFile(result.QueueName, result.CorrelationId)))
                    {
                        if (this.m_watchers.TryGetValue(result.QueueName, out var callbacks))
                        {
                            foreach (var cb in callbacks.ToArray())
                            {
                                cb(result);
                            }
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    this.m_resetEvent.Reset();
                }
            });
            this.m_listenerThread.IsBackground = true;
            this.m_listenerThread.Name = "FileSystemListener";
            this.m_listenerThread.Start(m_ListenThreadCancellationTokenSource.Token);

        }

        /// <summary>
        /// De-queue the object
        /// </summary>
        public Queue.DispatcherQueueEntry Dequeue(string queueName)
        {
            return this.DequeueById(queueName, null);
        }

        /// <summary>
        /// Determines whether the specified file is locked
        /// </summary>
        private bool IsFileLocked(String fileName, out bool isEmpty)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                isEmpty = stream.Length == 0;
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                isEmpty = false;
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// Dequeue by identifier
        /// </summary>
        public Queue.DispatcherQueueEntry DequeueById(string queueName, string correlationId)
        {
            try
            {
                if (String.IsNullOrEmpty(queueName))
                {
                    throw new ArgumentNullException(nameof(queueName));
                }

                // Open the queue
                this.Open(queueName);
                var queueFile = this.GetQueueFile(queueName, correlationId);
                if (String.IsNullOrEmpty(queueFile))
                {
                    return null;
                }

                bool isEmpty = false;
                while (this.IsFileLocked(queueFile, out isEmpty))
                {
                    Thread.Sleep(100);
                }
                if (isEmpty)
                {
                    File.Delete(queueFile);
                    return this.Dequeue(queueName);
                }

                this.m_tracer.TraceInfo("Will dequeue {0}", Path.GetFileNameWithoutExtension(queueFile));
                QueueEntry retVal = null;
                try
                {
                    retVal = this.ReadQueueEntry(queueFile);

                }
                finally
                {
                    File.Delete(queueFile);
                }

                return new Core.Queue.DispatcherQueueEntry(Path.GetFileNameWithoutExtension(queueFile), queueName, retVal.CreationTime, retVal.Type, retVal.ToObject());
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error de-queueing {0} - {1}", queueName, e);

                return null;
            }
        }

        /// <summary>
        /// Read a queue entry
        /// </summary>
        private QueueEntry ReadQueueEntry(string queueFile)
        {
            using (var fs = File.OpenRead(queueFile))
            {
                var iv = new byte[16];
                fs.Read(iv, 0, iv.Length);
                using (var cs = this.m_symmetricCrypto.CreateDecryptingStream(fs, this.m_symmetricCrypto.GetContextKey(), iv))
                {
                    return QueueEntry.Load(cs);
                }
            }
        }

        /// <summary>
        /// Get queue file
        /// </summary>
        private string GetQueueFile(string queueName, string correlationId)
        {

            String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);

            // Serialize
            String queueFile = null;

            if (String.IsNullOrEmpty(correlationId))
            {
                queueFile = Directory.EnumerateFiles(queueDirectory).FirstOrDefault();
            }
            else
            {
                queueFile = Path.Combine(queueDirectory, correlationId);
            }

            if (queueFile == null || !File.Exists(queueFile))
            {
                return null;
            }
            return queueFile;
        }

        /// <summary>
        /// Queue an item to the queue
        /// </summary>
        public void Enqueue(string queueName, object data)
        {
            if (String.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }
            else if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Open the queue
            this.Open(queueName);

            String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);

            // Serialize
            var tick = Interlocked.Increment(ref m_ctr);
            String fname = tick.ToString("00000000000000000000"),
                filePath = Path.Combine(queueDirectory, fname);
            // Prevent dups
            while (File.Exists(filePath))
            {
                tick++;
                fname = tick.ToString("00000000000000000000");
                filePath = Path.Combine(queueDirectory, fname);
            }

            using (var fs = File.Create(filePath))
            {
                var iv = this.m_symmetricCrypto.GenerateIV();
                fs.Write(iv, 0, iv.Length);
                using (var cs = this.m_symmetricCrypto.CreateEncryptingStream(fs, this.m_symmetricCrypto.GetContextKey(), iv))
                {
                    QueueEntry.Create(data).Save(cs);
                }
            }

            this.NotifyQueuePush(queueName, Path.GetFileNameWithoutExtension(filePath));
            this.m_tracer.TraceVerbose("Successfully queued {0}", fname);
        }

        /// <summary>
        /// Notify queue push
        /// </summary>
        private void NotifyQueuePush(string queueName, string correlationId)
        {
            this.m_notificationQueue.Enqueue(new DispatcherMessageEnqueuedInfo(queueName, correlationId));
            this.m_resetEvent.Set();
        }

        /// <summary>
        /// Create a directory and subscribe to it
        /// </summary>
        public void Open(string queueName)
        {
            if (this.m_watchers?.ContainsKey(queueName) == true)
            {
                return; // already open
            }

            String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);
            if (!Directory.Exists(queueDirectory))
            {
                Directory.CreateDirectory(queueDirectory);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (!this.m_disposed)
            {
                this.m_disposed = true;

                this.m_watchers.Clear();
                this.m_watchers = null;
                m_ListenThreadCancellationTokenSource.Cancel();

                //try
                //{
                //    this.m_listenerThread.Abort();
                //}
                //catch (PlatformNotSupportedException)
                //{
                //    //TODO: We need to properly cancel the threads using a cancellationtoken.
                //}
            }
        }

        /// <summary>
        /// Move the specified entry
        /// </summary>
        public Queue.DispatcherQueueEntry Move(Queue.DispatcherQueueEntry entry, string toQueue)
        {
            var oldEntryPath = Path.Combine(this.m_configuration.QueuePath, entry.SourceQueue, entry.CorrelationId);
            var newEntryPath = Path.Combine(this.m_configuration.QueuePath, toQueue, entry.CorrelationId);
            File.Move(oldEntryPath, newEntryPath);

            // Call callbacks
            this.NotifyQueuePush(toQueue, entry.CorrelationId);
            return new Core.Queue.DispatcherQueueEntry(entry.CorrelationId, toQueue, DateTime.Now, entry.Label, entry.Body);
        }

        /// <summary>
        /// Get all queues
        /// </summary>
        public IEnumerable<DispatcherQueueInfo> GetQueues()
        {
            foreach (var d in Directory.GetDirectories(this.m_configuration.QueuePath))
            {
                var di = new DirectoryInfo(d);
                yield return new DispatcherQueueInfo() { Id = Path.GetFileName(d), Name = Path.GetFileName(d), QueueSize = di.EnumerateFiles().Count(), CreationTime = di.CreationTime };
            }
        }

        /// <summary>
        /// Get all queue entries
        /// </summary>
        public IEnumerable<Queue.DispatcherQueueEntry> GetQueueEntries(string queueName)
        {
            foreach (var f in Directory.EnumerateFiles(Path.Combine(this.m_configuration.QueuePath, queueName)))
            {
                QueueEntry entry = null;
                try
                {
                    entry = this.ReadQueueEntry(f);
                }
                catch
                {
                    continue;
                }
                yield return new Core.Queue.DispatcherQueueEntry(Path.GetFileNameWithoutExtension(f), queueName, entry.CreationTime, entry.Type, entry.XmlData);
            }
        }

        /// <summary>
        /// Purge the file system queue
        /// </summary>
        public void Purge(string queueName)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageDispatcherQueues);

            try
            {
                var filesToRemove = Directory.EnumerateFiles(Path.Combine(this.m_configuration.QueuePath, queueName));
                foreach (var f in filesToRemove)
                {
                    File.Delete(f);
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error purging {0} - {1}", queueName, e);
                throw new DataPersistenceException($"Cannot purge {queueName}", e);
            }
        }

        /// <summary>
        /// Get a queue entry
        /// </summary>

        public DispatcherQueueEntry GetQueueEntry(string queueName, string correlationId)
        {
            var filePath = Path.Combine(this.m_configuration.QueuePath, queueName, correlationId);
            if (File.Exists(filePath))
            {
                var entry = this.ReadQueueEntry(filePath);
                return new DispatcherQueueEntry(correlationId, queueName, entry.CreationTime, entry.Type, entry.XmlData);
            }
            throw new KeyNotFoundException($"{queueName}\\{correlationId} doesn't exist");
        }

        /// <summary>
        /// Subscribe to the queue
        /// </summary>
        public void SubscribeTo(string queueName, DispatcherQueueCallback callback)
        {
            if (!this.m_watchers.TryGetValue(queueName, out var dispatcher))
            {
                dispatcher = new List<DispatcherQueueCallback>();
                this.m_watchers.TryAdd(queueName, dispatcher);
            }

            dispatcher.Add(callback);
        }

        /// <summary>
        /// Remove subscriptions
        /// </summary>
        public void UnSubscribe(string queueName, DispatcherQueueCallback callback)
        {
            if (this.m_watchers != null && this.m_watchers.TryGetValue(queueName, out var queueWatcher))
            {
                queueWatcher.Remove(callback);
            }
        }
    }
}