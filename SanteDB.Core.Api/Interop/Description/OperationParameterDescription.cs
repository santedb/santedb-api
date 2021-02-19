using System;

namespace SanteDB.Core.Interop.Description
{

    /// <summary>
    /// Parameter location
    /// </summary>
    public enum OperationParameterLocation
    {
        /// <summary>
        /// Parameter is in the body
        /// </summary>
        Body,
        /// <summary>
        /// Parameter is in the URL
        /// </summary>
        Path,
        /// <summary>
        /// Parameter is in the query
        /// </summary>
        Query
    }

    /// <summary>
    /// A single parameter which is expressed to the service
    /// </summary>
    public class OperationParameterDescription : ResourcePropertyDescription
    {
        /// <summary>
        /// Resource property description
        /// </summary>
        public OperationParameterDescription(String name, Type type, OperationParameterLocation location) : base (name, type)
        {
            this.Location = location;
        }

        /// <summary>
        /// Creates a new resoure 
        /// </summary>
        public OperationParameterDescription(String name, ResourceDescription resourceType, OperationParameterLocation location) : base(name, resourceType)
        {
            this.Location = location;
        }

        /// <summary>
        /// Gets the location of the parameter
        /// </summary>
        public OperationParameterLocation Location { get; }

    }
}