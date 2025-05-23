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
using SanteDB.Core.Model.Query;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Notifications.Email;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model;
using System.Reflection;
using SanteDB.Core.Model.Entities;

namespace SanteDB.Core.Jobs
{
    class RapidProData
    {
        public Guid Contact { get; set; }
        public string Text { get; set; }

        private readonly string RAPID_PRO_API_KEY = "";
    }

    /// <summary>
    /// </summary>
    public class NotificationSendingJob : IJob
    {
        private readonly IJobManagerService m_jobManager;

        private readonly IJobStateManagerService m_jobStateManager;

        private readonly IRepositoryService<NotificationInstance> m_notificationRepositoryService;

        private readonly IEmailService m_emailService;

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
        public NotificationSendingJob(IJobStateManagerService jobStateManagerService, IJobManagerService jobManagerService, IRepositoryService<NotificationInstance> repositoryService, IEmailService emailService)
        {
            this.m_jobManager = jobManagerService;
            this.m_jobStateManager = jobStateManagerService;
            this.m_notificationRepositoryService = repositoryService;
            m_emailService = emailService;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);

        }

        public async Task PostAsync(HttpClient httpClient, NotificationInstance notificationInstance)
        {
            // retrieve tags for channel types
            var channelTypes = notificationInstance.NotificationTemplate.Tags.Split(',');

            foreach (var channel in channelTypes)
            {
                switch (channel)
                {
                    case "email":
                        var emailMessage = new EmailMessage()
                        {
                            ToAddresses = new List<string>(),
                            FromAddress = "example@example.com",
                            Subject = notificationInstance.NotificationTemplate.Contents[0].Subject,
                            Body = notificationInstance.NotificationTemplate.Contents[0].Body
                        };
                        this.m_emailService.SendEmail(emailMessage);
                        Console.WriteLine("email sent");
                        Console.WriteLine(emailMessage.Body);
                        break;
                    case "sms":
                        Console.WriteLine("send a text message");
                        break;
                    case "facebook":
                        var data = new RapidProData
                        {
                            Contact = new Guid(""),
                            Text = notificationInstance.NotificationTemplate.Contents[0].Body
                        }; 
                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync( "https://app.rapidpro.io/api/v2/messages.json", content);

                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"{jsonResponse}\n");
                        break;
                    default:
                        Console.WriteLine("unknown channel");
                        break;
                }
            }
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
                    enabledNotifications.ForEach(async notification =>
                    {
                        if (!this.m_cancelRequested)
                        {
                            // retrieve all contacts
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "CLHM2PFCZS4873C4YLAFARGB4XAJCWDSJHLZSEXH");
                            httpClient.DefaultRequestHeaders.Host = "app.rapidpro.io";
                            var contactList =  httpClient.GetAsync("https://app.rapidpro.io/api/v2/contacts.json").GetAwaiter().GetResult();
                            var channelTypes =  httpClient.GetAsync(" https://app.rapidpro.io/api/v2/channels.json").GetAwaiter().GetResult();

                            var triggerExpression = QueryExpressionParser.BuildLinqExpression<NotificationInstance>(notification.TriggerExpression);
                            var triggerMethod = triggerExpression.Compile();

                            var isNotificationDue = triggerMethod(notification);
                            if (isNotificationDue)
                            {
                                var entityTypeConcept = ApplicationServiceContext.Current.GetService<IRepositoryService<Concept>>().Get(notification.EntityTypeKey);
                                var type = typeof(IdentifiedData).Assembly.ExportedTypes.FirstOrDefault(c => c.GetCustomAttributes<ClassConceptKeyAttribute>().Any(x => x.ClassConcept == entityTypeConcept.Key.ToString()));

                                var entityRepositoryService = ApplicationServiceContext.Current.GetService(typeof(IRepositoryService<>).MakeGenericType(type)) as IRepositoryService;
                                var filterExpression = QueryExpressionParser.BuildLinqExpression(type, notification.FilterExpression.ParseQueryString());
                                var filteredEntities = entityRepositoryService.Find(filterExpression).Cast<Entity>().ToArray();

                                notification.LastSentAt = DateTime.Now;
                                this.m_notificationRepositoryService.Save(notification);

                                filteredEntities.ForEach(entity =>
                                {
                                    PostAsync(httpClient, notification).GetAwaiter().GetResult();
                                    Console.WriteLine($"Notification Sent for Entity: {entity.Key}");
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
