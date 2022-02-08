namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Implementations of this class are expected to be able to transform a single input object
    /// into a single output object 
    /// </summary>
    /// <remarks>
    /// Implementations of this class may perform terminology lookups, parsing, translation etc.
    /// </remarks>
    public interface IForeignDataTransform
    {

        /// <summary>
        /// Gets the name of the transform
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Transform data from the source foreign data object to an appropriate type
        /// </summary>
        /// <param name="input">The input object</param>
        /// <param name="args">The arguments to the transformer (context specific)</param>
        /// <returns>The transformed object</returns>
        object Transform(object input, params object[] args);
    }
}