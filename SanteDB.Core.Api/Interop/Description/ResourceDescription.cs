using System.Collections.Generic;

namespace SanteDB.Core.Interop.Description
{
    /// <summary>
    /// Gets the resource description 
    /// </summary>
    public class ResourceDescription
    {

        /// <summary>
        /// Creates a new resource description
        /// </summary>
        public ResourceDescription(string name, string description)
        {
            this.Name = name;
            this.Description = description;
            this.Properties = new List<ResourcePropertyDescription>();

        }
        /// <summary>
        /// Gets the name of the description
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the resrouce
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the properties in the description
        /// </summary>
        public IList<ResourcePropertyDescription>  Properties { get;  }


    }
}