using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// A persistent queue service that uses the file system (use when there's no other infrastructure)
    /// </summary>
    public class FileSystemDispatcherQueueService : IDispatcherQueueManagerService, IDisposable
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "File System Message Queue";

        // Disposed already?
        private bool m_disposed = false;

        /// <summary>
        /// Queue entry
        /// </summary>
        [XmlType(nameof(QueueEntry), Namespace = "http://santedb.org/fsqueue")]
        [XmlRoot(nameof(QueueEntry), Namespace = "http://santedb.org/fsqueue")]
        public class QueueEntry
        {
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
                    xsz.Serialize(ms, data);
                    return new QueueEntry()
                    {
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

        // Queue root directory
        private FileSystemDispatcherQueueConfigurationSection m_configuration;

        // Watchers
        private Dictionary<String, IDisposable> m_watchers = new Dictionary<string, IDisposable>();

        /// <summary>
        /// Queue file
        /// </summary>
        private Tracer m_tracer = Tracer.GetTracer(typeof(FileSystemDispatcherQueueService));

        // Pep service
        private readonly IPolicyEnforcementService m_pepService;

        /// <summary>
        /// Initializes the file system queue
        /// </summary>
        public FileSystemDispatcherQueueService(IConfigurationManager configurationManager, IPolicyEnforcementService pepService)
        {
            this.m_configuration = configurationManager.GetSection<FileSystemDispatcherQueueConfigurationSection>();
            if (!Directory.Exists(this.m_configuration.QueuePath))
                Directory.CreateDirectory(this.m_configuration.QueuePath);
            this.m_pepService = pepService;
        }

        /// <summary>
        /// De-queue the object
        /// </summary>
        public Queue.DispatcherQueueEntry Dequeue(string queueName)
        {
            return this.DequeueById(queueName, null);
        }

        /// <summary>
        /// Dequeue by identifier
        /// </summary>
        public Queue.DispatcherQueueEntry DequeueById(string queueName, string correlationId)
        {
            try
            {
                if (String.IsNullOrEmpty(queueName))
                    throw new ArgumentNullException(nameof(queueName));

                // Open the queue
                this.Open(queueName);

                String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);

                // Serialize
                String queueFile = null;

                if (String.IsNullOrEmpty(correlationId))
                {
                    queueFile = Directory.GetFiles(queueDirectory).FirstOrDefault();
                }
                else
                {
                    queueFile = Path.Combine(queueDirectory, correlationId);
                }

                if (queueFile == null || !File.Exists(queueFile)) return null;

                this.m_tracer.TraceInfo("Will dequeue {0}", Path.GetFileNameWithoutExtension(queueFile));
                QueueEntry retVal = null;
                using (var fs = File.OpenRead(queueFile))
                {
                    retVal = QueueEntry.Load(fs);
                }
                File.Delete(queueFile);
                return new Core.Queue.DispatcherQueueEntry(Path.GetFileNameWithoutExtension(queueFile), queueName, retVal.CreationTime, retVal.Type, retVal.ToObject());
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error de-queueing {0} - {1}", queueName, e);
                return null;
            }
        }

        /// <summary>
        /// Queue an item to the queue
        /// </summary>
        public void Enqueue(string queueName, object data)
        {
            if (String.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(nameof(queueName));
            else if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Open the queue
            this.Open(queueName);

            String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);

            // Serialize
            long tick = DateTime.Now.Ticks;
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
                QueueEntry.Create(data).Save(fs);
            this.m_tracer.TraceInfo("Successfulled queued {0}", fname);
        }

        /// <summary>
        /// Create a directory and subscribe to it
        /// </summary>
        public void Open(string queueName)
        {
            if (this.m_watchers.ContainsKey(queueName))
                return; // already open

            String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);
            if (!Directory.Exists(queueDirectory))
                Directory.CreateDirectory(queueDirectory);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (!this.m_disposed)
            {
                this.m_disposed = true;
                foreach (var itm in this.m_watchers)
                {
                    this.m_tracer.TraceInfo("Disposing queue {0}", itm.Key);
                    itm.Value.Dispose();
                }
                this.m_watchers.Clear();
                this.m_watchers = null;
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
                yield return new DispatcherQueueInfo() { Id = Path.GetFileName(d), Name = Path.GetFileName(d), QueueSize = di.GetFiles().Length, CreationTime = di.CreationTime };
            }
        }

        /// <summary>
        /// Get all queue entries
        /// </summary>
        public IEnumerable<Queue.DispatcherQueueEntry> GetQueueEntries(string queueName)
        {
            foreach (var f in Directory.GetFiles(Path.Combine(this.m_configuration.QueuePath, queueName)))
            {
                QueueEntry entry = null;
                try
                {
                    using (var fs = File.OpenRead(f))
                        entry = QueueEntry.Load(fs);
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
                var filesToRemove = Directory.GetFiles(Path.Combine(this.m_configuration.QueuePath, queueName));
                foreach (var f in filesToRemove)
                    File.Delete(f);
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
                using (var fs = File.OpenRead(filePath))
                {
                    var entry = QueueEntry.Load(fs);
                    return new DispatcherQueueEntry(correlationId, queueName, entry.CreationTime, entry.Type, entry.XmlData);
                }
            }
            throw new KeyNotFoundException($"{queueName}\\{correlationId} doesn't exist");
        }

        /// <summary>
        /// Subscribe to the queue
        /// </summary>
        public void SubscribeTo(string queueName, DispatcherQueueCallback callback)
        {
            // Watchers
            lock (this.m_watchers)
            {
                String queueDirectory = Path.Combine(this.m_configuration.QueuePath, queueName);

                var fsWatch = new FileSystemWatcher(queueDirectory, "*");
                fsWatch.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                fsWatch.Created += (o, e) =>
                {
                    try
                    {
                        callback(new DispatcherMessageEnqueuedInfo(queueName, Path.GetFileNameWithoutExtension(e.FullPath)));
                    }
                    catch (Exception ex)
                    {
                        this.m_tracer.TraceEvent(EventLevel.Error, "FileSystem Watcher reported error on queue (Changed) -> {0}", ex);
                    }
                };
                fsWatch.Changed += (o, e) =>
                {
                    try
                    {
                        callback(new DispatcherMessageEnqueuedInfo(queueName, Path.GetFileNameWithoutExtension(e.FullPath)));
                    }
                    catch (Exception ex)
                    {
                        this.m_tracer.TraceEvent(EventLevel.Error, "FileSystem Watcher reported error on queue (Changed) -> {0}", ex);
                    }
                };
                fsWatch.EnableRaisingEvents = true;
                this.m_watchers.Add(queueName, fsWatch);

                this.m_tracer.TraceInfo("Opening queue {0}... Exhausing existing items...", queueDirectory);

                // If there's anything in the directory notify
                if (Directory.GetFiles(queueDirectory, "*").Any())
                {
                    callback(new DispatcherMessageEnqueuedInfo(queueName, Path.GetFileNameWithoutExtension("*")));
                }
            }
        }

        /// <summary>
        /// Remove subscriptions
        /// </summary>
        public void UnSubscribe(string queueName, DispatcherQueueCallback callback)
        {
            if (this.m_watchers.TryGetValue(queueName, out var queueWatcher))
            {
                queueWatcher.Dispose();
            }
        }
    }
}