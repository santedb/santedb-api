/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// File notification template service
    /// </summary>
    [ServiceProvider("File System based Notification Template Repository", Configuration = typeof(FileSystemNotificationTemplateConfigurationSection))]
    public class FileNotificationTemplateRepository : INotificationTemplateRepository
    {
        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(FileNotificationTemplateRepository));

        // Configuration
        private FileSystemNotificationTemplateConfigurationSection m_configuration;

        /// <summary>
        /// File notification repository
        /// </summary>
        public FileNotificationTemplateRepository(IConfigurationManager configurationManager)
        {
            m_configuration = configurationManager.GetSection<FileSystemNotificationTemplateConfigurationSection>();
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "File System Bsaed Notification Template Repository";

        // Lock box
        private object m_lock = new object();

        // Repository
        private List<NotificationTemplate> m_repository = new List<NotificationTemplate>();

        /// <summary>
        /// Initialize
        /// </summary>
        public FileNotificationTemplateRepository()
        {
            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                try
                {
                    m_tracer.TraceInfo("Scanning {0}", m_configuration.RepositoryRoot);
                    if (!Directory.Exists(m_configuration.RepositoryRoot))
                    {
                        Directory.CreateDirectory(m_configuration.RepositoryRoot);
                    }

                    Directory.GetFiles(m_configuration.RepositoryRoot, "*.xml", SearchOption.AllDirectories)
                        .ToList().ForEach(f =>
                        {
                            try
                            {
                                lock (m_lock)
                                {
                                    using (var fs = File.OpenRead(f))
                                    {
                                        m_repository.Add(NotificationTemplate.Load(fs));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                m_tracer.TraceWarning("Skipping {0} - {1}", f, ex.Message);
                            }
                        });
                }
                catch (Exception ex)
                {
                    m_tracer.TraceError("Error loading templates - {0}", ex);
                }
            };
        }

        /// <summary>
        /// Find the specified templates
        /// </summary>
        public IEnumerable<NotificationTemplate> Find(Expression<Func<NotificationTemplate, bool>> filter)
        {
            lock (m_lock)
            {
                return m_repository.Where(filter.Compile()).ToList();
            }
        }

        /// <summary>
        /// Get the specified template
        /// </summary>
        public NotificationTemplate Get(string id, string lang)
        {
            lock (m_lock)
            {
                return m_repository.FirstOrDefault(o => o.Id == id && o.Language == lang);
            }
        }

        /// <summary>
        /// Insert the specified template
        /// </summary>
        public NotificationTemplate Insert(NotificationTemplate template)
        {
            if (Get(template.Id, template.Language) != null)
            {
                throw new DuplicateNameException($"Template {template.Id} for language {template.Language} already exists");
            }

            return Update(template);
        }

        /// <summary>
        /// Update the specified template
        /// </summary>
        public NotificationTemplate Update(NotificationTemplate template)
        {
            try
            {
                lock (m_lock)
                {
                    m_repository.RemoveAll(o => o.Id == template.Id && o.Language == template.Language);
                    m_repository.Add(template);
                }

                var fileName = Path.Combine(m_configuration.RepositoryRoot, template.Language ?? "default", template.Id + ".xml");
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }

                using (var fs = File.Create(fileName))
                {
                    return template.Save(fs);
                }
            }
            catch (Exception e)
            {
                m_tracer.TraceError("Error updating the specified template - {0}", e);
                throw new Exception($"Error inserting {template.Id}", e);
            }
        }
    }
}