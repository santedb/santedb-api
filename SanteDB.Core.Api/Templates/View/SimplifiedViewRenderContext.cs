using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A simplified rendering context
    /// </summary>
    public class SimplifiedViewRenderContext
    {
        // The writer
        private readonly XmlWriter m_writer;
        private readonly SimplifiedViewComponent m_component;
        private readonly SimplifiedViewRenderContext m_parent;

        /// <summary>
        /// Create a new render context
        /// </summary>
        private SimplifiedViewRenderContext(XmlWriter writer, SimplifiedViewComponent component, SimplifiedViewRenderContext parentContext = null)
        {
            this.m_writer = writer;
            this.m_component = component;
            this.m_parent = parentContext;
        }


        /// <summary>
        /// Gets the HTML writer
        /// </summary>
        public XmlWriter HtmlWriter => this.m_writer;

        /// <summary>
        /// Gets the component that created this context
        /// </summary>
        public SimplifiedViewComponent Component => this.m_component;

        /// <summary>
        /// Gets the parent
        /// </summary>
        public SimplifiedViewRenderContext Parent => this.m_parent;

        /// <summary>
        /// Find parent components
        /// </summary>
        /// <typeparam name="TComponent">The type of parent component to find</typeparam>
        public IEnumerable<TComponent> FindParent<TComponent>() where TComponent: SimplifiedViewComponent
        {
            var ctx = this;
            while(ctx != null)
            {
                if(ctx.Component is TComponent tc)
                {
                    yield return tc;
                }
                ctx = ctx.Parent;
            }
        }

        /// <summary>
        /// Create a child context
        /// </summary>
        /// <param name="component">The component which is creating the child context</param>
        public SimplifiedViewRenderContext CreateChildContext(SimplifiedViewComponent component) => new SimplifiedViewRenderContext(this.m_writer, component, this);

        /// <summary>
        /// Create a render context
        /// </summary>
        internal static SimplifiedViewRenderContext Create(XmlWriter writer, SimplifiedViewComponent component) => new SimplifiedViewRenderContext(writer, component);
    }
}
