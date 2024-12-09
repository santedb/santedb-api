using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Represents a simplified input control
    /// </summary>
    public interface ISimplifiedInputControl
    {

        /// <summary>
        /// Render the control to the specified <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The XML writer where the output should be written</param>
        void Render(XmlWriter writer);
    }
}
