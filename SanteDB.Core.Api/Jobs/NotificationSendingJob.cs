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
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Notifications.RapidPro;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// </summary>
    public class NotificationSendingJob : IJob
    {
        private readonly IJobManagerService m_jobManager;

        private readonly IJobStateManagerService m_jobStateManager;

        private readonly IRepositoryService<NotificationInstance> m_notificationRepositoryService;
        private readonly IRepositoryService<NotificationTemplate> m_notificationTemplateService;
        private readonly IRepositoryService<NotificationTemplateParameter> m_notificationTemplateParametersService;

        private readonly IEmailService m_emailService;

        private readonly INotificationTemplateFiller m_notificationTemplateFiller;

        private bool m_cancelRequested = false;

        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Job Id
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("A5C97883-A21E-4C33-B428-E69002B7A453");

        /// <summary>
        /// API KEY to be moved to the configuration
        /// </summary>
        public static readonly string RAPIDPRO_API_KEY = "CLHM2PFCZS4873C4YLAFARGB4XAJCWDSJHLZSEXH";

        /// <summary>
        /// Endpoints for the RapidPro API
        /// </summary>
        public static readonly string RAPIDPRO_CONTACTS_ENDPOINT = "https://app.rapidpro.io/api/v2/contacts.json";
        public static readonly string RAPIDPRO_MESSAGE_ENDPOINT = "https://app.rapidpro.io/api/v2/messages.json";

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
        public NotificationSendingJob(IJobStateManagerService jobStateManagerService, IJobManagerService jobManagerService, IRepositoryService<NotificationInstance> repositoryService, IEmailService emailService, INotificationTemplateFiller notificationTemplateFiller, IRepositoryService<NotificationTemplate> notificationTemplateService, IRepositoryService<NotificationTemplateParameter> notificationTemplateParametersService)
        {
            this.m_jobManager = jobManagerService;
            this.m_jobStateManager = jobStateManagerService;
            this.m_notificationRepositoryService = repositoryService;
            this.m_emailService = emailService;
            this.m_notificationTemplateFiller = notificationTemplateFiller;
            this.m_notificationTemplateService = notificationTemplateService;
            this.m_notificationTemplateParametersService = notificationTemplateParametersService;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);

        }

        private async Task PostAsync(NotificationInstance notificationInstance, List<RapidProContact> contactList, Entity notificationEntity)
        {
            // retrieve template data
            var template =  this.m_notificationTemplateService.Get(notificationInstance.NotificationTemplateKey);
            notificationInstance.NotificationTemplate = template;

            // retrieve tags for channel types
            var channelTypes = template.Tags.Split(',');

            // fill in the template
            var model = new Dictionary<string, object>();

            foreach (var parameter in notificationInstance.InstanceParameters)
            {
                var paramName = this.m_notificationTemplateParametersService.Get(parameter.TemplateParameterKey);
                model.Add(paramName.Name, parameter.Expression);
            }

            var filledTemplate = this.m_notificationTemplateFiller.FillTemplate(notificationInstance, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, model);

            foreach (var channel in channelTypes)
            {
                switch (channel)
                {
                    case "facebook":
                        //find a contact by the name
                        var computedName = notificationEntity.Names;
                        //var selectedEntity = contactList.Find(entity => entity.name.Contains(computedName));

                        var data = new
                        {
                            Contact = new Guid("fafb5336-a706-4765-9025-0c83ccae6b3e"),
                            Text = filledTemplate.Body
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync(RAPIDPRO_MESSAGE_ENDPOINT, content);

                        var jsonResponse = await response.Content.ReadAsStringAsync();
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

                using (AuthenticationContext.EnterSystemContext())
                {
                    var enabledNotifications = this.m_notificationRepositoryService.Find(i => i.StateKey != Guid.Parse("1E029E45-734E-4514-9CA4-E1E487883562")).ToArray();
                    enabledNotifications.ForEach(async notification =>
                    {
                        if (!this.m_cancelRequested)
                        {
                            httpClient.DefaultRequestHeaders.Clear();
                            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", RAPIDPRO_API_KEY);
                            //HACK SOLUTION: The Rapid Pro API returns 403 to any request if UserAgent Header is empty
                            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Fiddler");
                            
                            var contactList = httpClient.GetAsync(RAPIDPRO_CONTACTS_ENDPOINT).GetAwaiter().GetResult();
                            var contacts = contactList.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var contactsObject = JsonConvert.DeserializeObject<List<RapidProContact>>(JObject.Parse(contacts)["results"].ToString());

                            var triggerExpression = QueryExpressionParser.BuildLinqExpression<NotificationInstance>(notification.TriggerExpression);
                            var triggerMethod = triggerExpression.Compile();

                            var isNotificationDue = triggerMethod(notification);
                            
                            if (isNotificationDue)
                            {
                                // HACK: Currently, we only have a reference to the entity key, but no reliable way to determine the Class Concept of that entity. As such, we retrieve the entity, then find the matching Class Concept bound to the entity, in order to determine the correct type to use when constructing the IRepositoryService instance as well as constructing the LINQ expression.
                                var entityTypeConcept = ApplicationServiceContext.Current.GetService<IRepositoryService<Concept>>().Get(notification.EntityTypeKey);
                                var type = typeof(IdentifiedData).Assembly.ExportedTypes.FirstOrDefault(c => c.GetCustomAttributes<ClassConceptKeyAttribute>().Any(x => x.ClassConcept == entityTypeConcept.Key.ToString()));

                                var entityRepositoryService = ApplicationServiceContext.Current.GetService(typeof(IRepositoryService<>).MakeGenericType(type)) as IRepositoryService;
                                var filterExpression = QueryExpressionParser.BuildLinqExpression(type, notification.FilterExpression.ParseQueryString());
                                var filteredEntities = entityRepositoryService.Find(filterExpression).Cast<Entity>().ToArray();

                                notification.LastSentAt = DateTimeOffset.Now;
                                this.m_notificationRepositoryService.Save(notification);

                                filteredEntities.ForEach(entity =>
                                {
                                    try
                                    {
                                        PostAsync( notification, contactsObject, entity).GetAwaiter().GetResult();
                                    }
                                    catch (Exception ex)
                                    {
                                        // inject jogger
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
