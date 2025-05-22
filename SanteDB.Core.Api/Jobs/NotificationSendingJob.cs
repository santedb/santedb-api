/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using Newtonsoft.Json;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Jobs
{
    class RapidProData
    {
        public Guid Contact { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// </summary>
    public class NotificationSendingJob : IJob
    {
        private readonly IJobManagerService m_jobManager;

        private readonly IJobStateManagerService m_jobStateManager;

        private readonly IRepositoryService<NotificationInstance> m_notificationRepositoryService;

        private bool m_cancelRequested = false;

        /// <summary>
        /// Job Id
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("A5C97883-A21E-4C33-B428-E69002B7A453");

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <inheritdoc/>
        public string Name => "Send Scheduled Notifications Job";

        /// <inheritdoc/>
        public string Description => "Sends any due scheduled notifications";

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => null;

        /// <inheritdoc/>
        public DateTime? LastStarted { get; private set; }

        /// <inheritdoc/>
        public DateTime? LastFinished { get; private set; }

        /// <summary>
        /// Dependency injected constructor
        /// </summary>
        public NotificationSendingJob(IJobStateManagerService jobStateManagerService, IJobManagerService jobManagerService, IRepositoryService<NotificationInstance> notificationRepositoryService)
        {
            this.m_jobManager = jobManagerService;
            this.m_jobStateManager = jobStateManagerService;
            this.m_notificationRepositoryService = notificationRepositoryService;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);

        }

        static async Task PostAsync(HttpClient httpClient)
        {
            var data = new RapidProData
            {
                Contact = Guid.Empty,
                Text = "test"
            };

            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(
                "https://app.rapidpro.io/api/v2/messages.json",
                content);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");
        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                this.m_jobStateManager.SetState(this, JobStateType.Running);
                this.m_cancelRequested = false;
                this.LastStarted = DateTime.Now;
                var httpClient = new HttpClient();

                using (AuthenticationContext.EnterSystemContext())
                {
                    var enabledNotifications = this.m_notificationRepositoryService.Find(i => i.StateKey != Guid.Parse("1E029E45-734E-4514-9CA4-E1E487883562")).ToArray();
                    enabledNotifications.ForEach(notification =>
                    {
                        if (!this.m_cancelRequested)
                        {
                            var triggerExpression = QueryExpressionParser.BuildLinqExpression<NotificationInstance>(notification.TriggerExpression);
                            var triggerMethod = triggerExpression.Compile();

                            var isNotificationDue = triggerMethod(notification);
                            if (isNotificationDue)
                            {
                                var entityType = notification.EntityType;
                                var entityRepository = ApplicationServiceContext.Current.GetService<IRepositoryService<Patient>>();

                                var filterExpression = QueryExpressionParser.BuildLinqExpression<Patient>(notification.FilterExpression);
                                var filterMethod = filterExpression.Compile();

                                notification.LastSentAt = DateTime.Now;
                                m_notificationRepositoryService.Save(notification);

                                var entities = entityRepository.Find(entity => true).ToArray();
                                entities.ForEach(entity =>
                                {
                                    if (filterMethod(entity))
                                    {
                                        // PostAsync(httpClient).GetAwaiter().GetResult();
                                        Console.WriteLine($"Notification Sent for {entity.Type}: {entity.Key}");
                                    }
                                });
                            }
                        }
                    });
                }

                if (this.m_cancelRequested)
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Cancelled);
                }
                else
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Completed);
                }

                this.LastFinished = DateTime.Now;
            }
            catch (Exception ex)
            {
                this.m_jobStateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
            }
        }
    }
}
