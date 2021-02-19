using System;

namespace SanteDB.Core.Interop.Description
{
    /// <summary>
    /// Represents a property descriptor
    /// </summary>
    public class ResourcePropertyDescription
    {


        /// <summary>
        /// Resource property description
        /// </summary>
        public ResourcePropertyDescription(String name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        /// <summary>
        /// Creates a new resoure 
        /// </summary>
        public ResourcePropertyDescription(String name, ResourceDescription resourceType)
        {
            this.Name = name;
            this.Type = typeof(ResourceDescription);
            this.ResourceType = resourceType;
        }

        /// <summary>
        /// Gets the name of the object
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// Gets the type of resource
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a resource description
        /// </summary>
        public ResourceDescription ResourceType { get; }

    }
}