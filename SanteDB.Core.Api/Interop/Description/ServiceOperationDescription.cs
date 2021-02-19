using System.Collections.Generic;
using System.Net;

namespace SanteDB.Core.Interop.Description
{
    /// <summary>
    /// A single operation on the service
    /// </summary>
    public class ServiceOperationDescription
    {

        /// <summary>
        /// Creates a new service description
        /// </summary>
        public ServiceOperationDescription(string verb, string path, string[] acceptsContentType, bool requiresAuth)
        {
            this.Responses = new Dictionary<HttpStatusCode, ResourceDescription>();
            this.Parameters = new List<OperationParameterDescription>();
            this.Verb = verb;
            this.Path = path;
            this.Accepts = this.Produces = acceptsContentType;
            this.Tags = new List<string>();
            this.RequiresAuth = requiresAuth;
        }

        /// <summary>
        /// Gets the verb for this description
        /// </summary>
        public string Verb { get; }

        /// <summary>
        /// Gets the responses
        /// </summary>
        public IDictionary<HttpStatusCode, ResourceDescription> Responses { get; }

        /// <summary>
        /// Gets the parameters
        /// </summary>
        public IList<OperationParameterDescription>  Parameters { get; }

        /// <summary>
        /// Gets the path description
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the type of content that this operation accepts
        /// </summary>
        public IEnumerable<string> Accepts { get; }

        /// <summary>
        /// Gets the type of content that this operation produces
        /// </summary>
        public IEnumerable<string> Produces { get; }

        /// <summary>
        /// Gets the tags for this object
        /// </summary>
        public IList<string> Tags { get; }

        /// <summary>
        /// Requires authorization
        /// </summary>
        public bool RequiresAuth { get; }
    }
}