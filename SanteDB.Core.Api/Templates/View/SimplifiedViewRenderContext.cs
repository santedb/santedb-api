/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2024-12-22
 */
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
