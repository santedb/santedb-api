using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SanteDB.Core.Templates.Definition;

namespace SanteDB.Core.Templates
{
    /// <summary>
    /// Represents a service which manages user defined templates
    /// </summary>
    /// <remarks>
    /// User defined templates are a combination of: 
    /// 1. A template definition (model template)
    /// 2. One or more views for the template representing:
    ///     - Detail View
    ///     - Summary View
    ///     - Back Entry
    ///     - Entry
    /// </remarks>
    public interface IDataTemplateManagementService : IServiceImplementation
    {

        /// <summary>
        /// Find a template definition based on the query provided
        /// </summary>
        /// <param name="query">The query that is to be executed</param>
        /// <returns>The result set representing the matching data template definition</returns>
        IQueryResultSet<DataTemplateDefinition> Find(Expression<Func<DataTemplateDefinition, bool>> query);

        /// <summary>
        /// Gets the specified data template by the UUID key of the template definition
        /// </summary>
        /// <param name="key">The key of the template definition</param>
        /// <returns>The defined template</returns>
        DataTemplateDefinition Get(Guid key);

        /// <summary>
        /// Adds or updates the specified <paramref name="definition"/> into the template manager
        /// </summary>
        /// <param name="definition">The template definition which is to be updated or added</param>
        /// <returns>The added or updated template definition</returns>
        DataTemplateDefinition AddOrUpdate(DataTemplateDefinition definition);

        /// <summary>
        /// Removes the specified template definition
        /// </summary>
        /// <param name="key">The key of the definition that is to be deleted.</param>
        /// <returns>The deleted or removed template definition</returns>
        DataTemplateDefinition Remove(Guid key);


    }
}
