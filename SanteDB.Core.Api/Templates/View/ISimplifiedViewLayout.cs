namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Marker interface for layout components
    /// </summary>
    public interface ISimplifiedViewLayout
    {
        /// <summary>
        /// Render implementation
        /// </summary>
        void Render(SimplifiedViewRenderContext context);
    }
}