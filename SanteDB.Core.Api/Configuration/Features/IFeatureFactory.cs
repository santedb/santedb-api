using System.Collections.Generic;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a class which can create other features
    /// </summary>
    public interface IFeatureFactory
    {

        /// <summary>
        /// Instructs the feactory to create and get features that it needs
        /// </summary>
        /// <returns></returns>
        IEnumerable<IFeature> GetFeatures();

    }
}
