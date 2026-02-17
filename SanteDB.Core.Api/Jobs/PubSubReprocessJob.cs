using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.PubSub;
using SanteDB.Core.PubSub.Broker;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Pub-sub reprocessing job for 
    /// </summary>
    public class PubSubReprocessJob : IJob
    {

        /// <summary>
        /// Job identifier constant
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("5A389F18-0170-4D73-A37A-EE99B2EB201E");

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(PubSubReprocessJob));

        private readonly IDispatcherQueueManagerService m_dispatchQueue;
        private readonly IPubSubLogService m_pubSubLog;
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IPubSubManagerService m_pubSubManager;
        private readonly IServiceProvider m_serviceProvider;
        private bool m_cancelRequested = false;

        /// <summary>
        /// DI re-process
        /// </summary>
        public PubSubReprocessJob(
            IDispatcherQueueManagerService dispatcherQueueService,
            IPubSubManagerService pubSubManager,
            IPubSubLogService pubSubLogService,
            IJobStateManagerService jobStateManager,
            IServiceProvider serviceProvider)
        {
            this.m_dispatchQueue = dispatcherQueueService;
            this.m_pubSubLog = pubSubLogService;
            this.m_jobStateManager = jobStateManager;
            this.m_pubSubManager = pubSubManager;
            this.m_serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <inheritdoc/>
        public string Name => "Re-Process Pub/Sub Subscriptions";

        /// <inheritdoc/>
        public string Description => "Forces the pub/sub broker to re-process any outstanding objects (or those changed in the database since last execution)";

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>()
        {
            {
                "subscriptionName", typeof(String)
            }
        };

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);
        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            if (parameters.Length != 1 || parameters[0] == null || String.Empty.Equals(parameters[0]))
            {
                this.m_jobStateManager.SetState(this, JobStateType.Cancelled, "subscriptionName is required");
                return;
            }

            this.m_cancelRequested = false;

            try
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Starting);
                    var subscription = this.m_pubSubManager.GetSubscriptionByName(parameters[0].ToString());
                    if (subscription == null)
                    {
                        throw new KeyNotFoundException(String.Format(ErrorMessages.OBJECT_NOT_FOUND, parameters[0]));
                    }
                    else if (!subscription.IsActive)
                    {
                        this.m_jobStateManager.SetState(this, JobStateType.Aborted, $"Subscription {subscription.Name} is not enabled");
                        return;
                    }

                    // Collect the data which needs to be re-processed
                    var repositoryType = typeof(IRepositoryService<>).MakeGenericType(subscription.ResourceType);
                    var repositoryService = this.m_serviceProvider.GetService(repositoryType) as IRepositoryService;

                    // Did the service resolve?
                    if (repositoryService == null)
                    {
                        throw new InvalidOperationException(String.Format(ErrorMessages.SERVICE_NOT_FOUND, repositoryType));
                    }

                    // Now we re-process
                    var baseExpression = String.Join("&", subscription.Filter).ParseQueryString();
                    if (subscription.NotBefore.HasValue)
                    {
                        baseExpression.Add("modifiedOn", $">={subscription.NotBefore.Value:o}");
                    }
                    if (subscription.NotAfter.HasValue)
                    {
                        baseExpression.Add("modifiedOn", $"<={subscription.NotAfter.Value:o}");
                    }

                    LambdaExpression filterExpression = QueryExpressionParser.BuildLinqExpression(subscription.ResourceType, baseExpression);
                    var resultSet = repositoryService.Find(filterExpression);
                    this.m_jobStateManager.SetState(this, JobStateType.Running);
                    long i = 0, totalRecords = resultSet.Count();
                    foreach (var itm in resultSet.OfType<IdentifiedData>())
                    {
                        if (this.m_cancelRequested)
                        {
                            this.m_jobStateManager.SetState(this, JobStateType.Cancelled, "User Cancelled");
                            return;
                        }
                        if (i++ % 10 == 0)
                        {
                            this.m_jobStateManager.SetProgress(this, $"Queueing {i} of {totalRecords}", (float)i / (float)totalRecords);
                        }

                        // Get the data and see if it has been updated or never sent
                        var pubSubRecord = this.m_pubSubLog.GetLastDispatch(subscription.Name, itm.Key.Value);

                        // We want to find the last successful record
                        if (pubSubRecord?.Outcome == Model.Audit.OutcomeIndicator.SeriousFail ||
                            pubSubRecord?.Outcome == Model.Audit.OutcomeIndicator.EpicFail)
                        {
                            pubSubRecord = this.m_pubSubLog.GetDispatches(subscription.Name, itm.Key.Value).Where(o => o.Outcome == Model.Audit.OutcomeIndicator.Success).OrderByDescending(o => o.DispatchTime).FirstOrDefault();
                        }

                        if (pubSubRecord == null ||
                            (itm is IVersionedData ivd && ivd.VersionSequence > pubSubRecord.VersionSequence ||
                            itm is INonVersionedData nvd && (nvd.UpdatedTime ?? nvd.CreationTime) >= pubSubRecord.DispatchTime))
                        {
                            // HACK: This should be done via a new interface to queue via the broker
                            this.m_dispatchQueue.Enqueue($"{PubSubBroker.QueueName}.{subscription.Name}", new PubSubNotifyQueueEntry(subscription.ResourceType, pubSubRecord == null ? PubSubEventType.Create : PubSubEventType.Update, itm));
                        }
                    }
                    this.m_jobStateManager.SetState(this, JobStateType.Completed);
                }

            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError("Error running re-process job: {0}", ex);
                this.m_jobStateManager.SetState(this, JobStateType.Aborted, $"Error re-processing pub/sub data - {ex.Message}");
            }
        }
    }
}
