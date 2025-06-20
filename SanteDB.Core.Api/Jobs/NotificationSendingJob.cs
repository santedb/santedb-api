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
using SanteDB.Core.Model.Constants;
using SharpCompress.Common;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Map;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// </summary>
    public class NotificationSendingJob : IJob
    {
        private readonly IJobStateManagerService m_jobStateManager;

        private readonly IRepositoryService<NotificationInstance> m_notificationRepositoryService;
        private readonly IRepositoryService<NotificationTemplate> m_notificationTemplateService;
        private readonly IRepositoryService<NotificationTemplateParameter> m_notificationTemplateParametersService;
        private readonly IRepositoryService<EntityTelecomAddress> m_entityTelecomAddressRepositoryService;

        private readonly Tracer m_tracer;

        private readonly INotificationTemplateFiller m_notificationTemplateFiller;

        private readonly INotificationService m_notificationService;

        private bool m_cancelRequested = false;

        /// <summary>
        /// Job Id
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("A5C97883-A21E-4C33-B428-E69002B7A453");

        /// <summary>
        /// Completed successfully state key
        /// </summary>
        private static readonly Guid COMPLETED_SUCCESSFULLY_STATE_KEY = Guid.Parse("2726BC79-A55A-4FEA-BE2C-627265872DB5");

        /// <summary>
        /// Completed with errors state key
        /// </summary>
        private static readonly Guid COMPLETED_WITH_ERRORS_STATE_KEY = Guid.Parse("92455ACD-ECC2-4E89-9227-94B77B27420D");

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
        public NotificationSendingJob(IJobStateManagerService jobStateManagerService, IRepositoryService<NotificationInstance> repositoryService, INotificationTemplateFiller notificationTemplateFiller, IRepositoryService<NotificationTemplate> notificationTemplateService, IRepositoryService<NotificationTemplateParameter> notificationTemplateParametersService, INotificationService notificationService, IRepositoryService<EntityTelecomAddress> entityTelecomAddressRepositoryService)
        {
            this.m_jobStateManager = jobStateManagerService;
            this.m_notificationRepositoryService = repositoryService;
            this.m_notificationTemplateFiller = notificationTemplateFiller;
            this.m_notificationTemplateService = notificationTemplateService;
            this.m_notificationTemplateParametersService = notificationTemplateParametersService;
            this.m_notificationService = notificationService;
            this.m_entityTelecomAddressRepositoryService = entityTelecomAddressRepositoryService;
            this.m_tracer = Tracer.GetTracer(this.GetType());
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);

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
                            var triggerExpression = QueryExpressionParser.BuildLinqExpression<NotificationInstance>(notification.TriggerExpression);
                            var triggerMethod = triggerExpression.Compile();

                            var isNotificationDue = triggerMethod(notification);
                            
                            if (isNotificationDue)
                            {
                                var entityType = MapUtil.GetModelTypeFromClassKey(notification.EntityTypeKey);

                                var entityRepositoryService = ApplicationServiceContext.Current.GetService(typeof(IRepositoryService<>).MakeGenericType(entityType)) as IRepositoryService;
                                var filterExpression = QueryExpressionParser.BuildLinqExpression(entityType, notification.FilterExpression.ParseQueryString());
                                var filteredEntities = entityRepositoryService.Find(filterExpression).Cast<Entity>().ToArray();

                                notification.LastSentAt = DateTimeOffset.Now;
                                notification.StateKey = COMPLETED_SUCCESSFULLY_STATE_KEY;

                                filteredEntities.ForEach(entity =>
                                {
                                    try
                                    {
                                        // retrieve template data
                                        var template = this.m_notificationTemplateService.Get(notification.NotificationTemplateKey);
                                        notification.NotificationTemplate = template;

                                        var targetExpression = QueryExpressionParser.BuildLinqExpression<EntityTelecomAddress>(notification.TargetExpression);

                                        var telecomAddress = this.m_entityTelecomAddressRepositoryService.Find(targetExpression).FirstOrDefault()?.IETFValue;

                                        // fill in the template
                                        var model = new Dictionary<string, object>();

                                        foreach (var parameter in notification.InstanceParameters)
                                        {
                                            model.Add(parameter.ParameterName, parameter.Expression);
                                        }

                                        var filledTemplate = this.m_notificationTemplateFiller.FillTemplate(notification, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, model);

                                        this.m_notificationService.SendNotification(new[] { telecomAddress }, filledTemplate.Subject, filledTemplate.Body);
                                    }
                                    catch (Exception ex)
                                    {
                                        notification.StateKey = COMPLETED_WITH_ERRORS_STATE_KEY;
                                        this.m_tracer.TraceError("Error sending notification for entity {0}: {1}", entity.Key, ex.ToHumanReadableString());
                                    }
                                });

                                this.m_notificationRepositoryService.Save(notification);
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
