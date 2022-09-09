using SanteDB.Core.Http;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Integration result arguments
    /// </summary>
    public class UpstreamIntegrationResultEventArgs : EventArgs
    {

        /// <summary>
        /// Creates a new integration result based on the response
        /// </summary>
        public UpstreamIntegrationResultEventArgs(IdentifiedData submitted, IdentifiedData result)
        {
            this.SubmittedData = submitted;
            this.ResponseData = result;
        }

        /// <summary>
        /// The data that was submitted to the server
        /// </summary>
        public IdentifiedData SubmittedData { get; set; }

        /// <summary>
        /// The data that the server responded with
        /// </summary>
        public IdentifiedData ResponseData { get; set; }

    }

    /// <summary>
    /// Query options to control data coming back from the server
    /// </summary>
    public class UpstreamIntegrationOptions
    {
        /// <summary>
        /// Gets or sets the If-Modified-Since header
        /// </summary>
        public DateTime? IfModifiedSince { get; set; }

        /// <summary>
        /// Gets or sets the If-None-Match
        /// </summary>
        public String IfNoneMatch { get; set; }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Lean
        /// </summary>
        public bool Lean { get; set; }

        /// <summary>
        /// Gets or sets the query identifier for the query
        /// </summary>
        public Guid QueryId { get; set; }

        /// <summary>
        /// Generates an event hander for the integration options
        /// </summary>
        public static EventHandler<RestRequestEventArgs> CreateRequestingHandler(UpstreamIntegrationOptions options)
        {
            return (o, e) =>
            {
                if (options == null) return;
                else if (options?.IfModifiedSince.HasValue == true)
                    e.AdditionalHeaders[HttpRequestHeader.IfModifiedSince] = options?.IfModifiedSince.Value.ToString();
                else if (!String.IsNullOrEmpty(options?.IfNoneMatch))
                    e.AdditionalHeaders[HttpRequestHeader.IfNoneMatch] = options?.IfNoneMatch;
                if (options?.Lean == true)
                    e.Query.Add("_lean", "true");
               
            };
        }
    }

    /// <summary>
    /// Represents an integration service which is responsible for sending and
    /// pulling data to/from remote sources as a configured device or application account principal 
    /// rather than an interactive user
    /// </summary>
    public interface IUpstreamIntegrationService : IServiceImplementation
    {

        /// <summary>
        /// Fired on response
        /// </summary>
        event EventHandler<RestResponseEventArgs> Responding;

        /// <summary>
        /// Progress has changed
        /// </summary>
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// The remote system has responsed
        /// </summary>
        event EventHandler<UpstreamIntegrationResultEventArgs> Responded;

        /// <summary>
        /// Find the specified filtered object
        /// </summary>
        IQueryResultSet<IdentifiedData> Find(Type modelType, NameValueCollection filter, UpstreamIntegrationOptions options = null);

        /// <summary>
        /// Find the specified filtered object
        /// </summary>
        IQueryResultSet<IdentifiedData> Find<TModel>(NameValueCollection filter, int offset, int? count, UpstreamIntegrationOptions options = null) where TModel : IdentifiedData;

        /// <summary>
        /// Instructs the integration service to locate a specified object(s)
        /// </summary>
        IQueryResultSet<IdentifiedData> Find<TModel>(Expression<Func<TModel, bool>> predicate, UpstreamIntegrationOptions options = null) where TModel : IdentifiedData;

        /// <summary>
        /// Instructs the integration service to retrieve the specified object
        /// </summary>
        IdentifiedData Get(Type modelType, Guid key, Guid? versionKey, UpstreamIntegrationOptions options = null);

        /// <summary>
        /// Gets a specified model.
        /// </summary>
        /// <typeparam name="TModel">The type of model data to retrieve.</typeparam>
        /// <param name="key">The key of the model.</param>
        /// <param name="versionKey">The version key of the model.</param>
        /// <param name="options">The integrations query options.</param>
        /// <returns>Returns a model.</returns>
        TModel Get<TModel>(Guid key, Guid? versionKey, UpstreamIntegrationOptions options = null) where TModel : IdentifiedData;

        /// <summary>
        /// Inserts specified data.
        /// </summary>
        /// <param name="data">The data to be inserted.</param>
        void Insert(IdentifiedData data);

        /// <summary>
        /// Determines whether the network is available.
        /// </summary>
        /// <returns>Returns true if the network is available.</returns>
        bool IsAvailable();

        /// <summary>
        /// Obsoletes specified data.
        /// </summary>
        /// <param name="data">The data to be obsoleted.</param>
        /// <param name="forceObsolete">Force the obsoletion</param>
        void Obsolete(IdentifiedData data, bool forceObsolete = false);

        /// <summary>
        /// Updates specified data.
        /// </summary>
        /// <param name="data">The data to be updated.</param>
        /// <param name="forceUpdate">When true, indicates that update should not do a safety check</param>
        void Update(IdentifiedData data, bool forceUpdate = false);
    }

}
