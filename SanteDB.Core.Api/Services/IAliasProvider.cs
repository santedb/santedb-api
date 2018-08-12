using SanteDB.Core.Model;
using SanteDB.Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents a component alias
    /// </summary>
    public struct ComponentAlias
    {
        /// <summary>
        /// Represents component aliases
        /// </summary>
        /// <param name="alias">The alias of the name</param>
        /// <param name="relevance">The relevance of the name alias</param>
        public ComponentAlias(string alias, double relevance)
        {
            this.Alias = alias;
            this.Relevance = relevance;
        }

        /// <summary>
        /// Gets the aliased name
        /// </summary>
        public String Alias { get; private set; }

        /// <summary>
        /// Gets the relevance of the alias
        /// </summary>
        public double Relevance { get; private set; }

    }

    /// <summary>
    /// Represents a provider for aliases
    /// </summary>
    public interface IAliasProvider
    {

        /// <summary>
        /// Gets the known alias names and score for the alias 
        /// </summary>
        IEnumerable<ComponentAlias> GetAlias(String name);
    }
}
