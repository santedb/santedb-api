using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data
{

    /// <summary>
    /// Data management link events
    /// </summary>
    public class DataManagementLinkEventArgs : EventArgs
    {

        /// <summary>
        /// Get the managed link that was altered
        /// </summary>
        public DataManagementLinkEventArgs(ITargetedAssociation targetedAssociation)
        {
            this.TargetedAssociation = targetedAssociation;
        }

        /// <summary>
        /// Gets the managed link that was impacted
        /// </summary>
        public ITargetedAssociation TargetedAssociation { get; }

    }

    /// <summary>
    /// Represents a specific data manager within a <see cref="IDataManagementPattern"/> which is responsible for resolving and linking together logical
    /// objects
    /// </summary>
    public interface IDataManagedLinkProvider<T>
        where T : IdentifiedData
    {
        /// <summary>
        /// Fired when a managed link is established
        /// </summary>
        event EventHandler<DataManagementLinkEventArgs> ManagedLinkEstablished;

        /// <summary>
        /// Fired when a managed link is removed
        /// </summary>
        event EventHandler<DataManagementLinkEventArgs> ManagedLinkRemoved;

        /// <summary>
        /// When a data management pattern (like MDM) masks or performs specialized linking in the database
        /// this method will allow callers to discern the true record.
        /// </summary>
        /// <param name="forSource">The record returned from the persistence layer</param>
        /// <returns>The resolved target object</returns>
        T ResolveManagedTarget(T forSource);

        /// <summary>
        /// When a data management pattern (like MDM) masks or performs specialized linking in the database
        /// and a target has been returned, this method will allow callers to discern the record in the database.
        /// </summary>
        /// <param name="forTarget">The record returned from the persistence layer</param>
        /// <returns>The resolved target object</returns>
        T ResolveManagedSource(T forTarget);

        /// <summary>
        /// Get the managed reference links for the collection of relationships
        /// </summary>
        /// <param name="forRelationships">The relationship collection on the object</param>
        /// <returns>The reference links on the object</returns>
        IEnumerable<ITargetedAssociation> FilterManagedReferenceLinks(IEnumerable<ITargetedAssociation> forRelationships);

        /// <summary>
        /// Add a managed reference link between <paramref name="sourceObject"/> and <paramref name="targetObject"/>
        /// </summary>
        /// <param name="sourceObject">The source object of the link</param>
        /// <param name="targetObject">The target object of the link</param>
        /// <returns>The created target</returns>
        ITargetedAssociation AddManagedReferenceLink(T sourceObject, T targetObject);

    }
}
