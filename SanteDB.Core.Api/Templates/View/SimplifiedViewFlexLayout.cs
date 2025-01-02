using Newtonsoft.Json;
using System.Xml.Serialization;
using ZstdSharp.Unsafe;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Direction of the stacking
    /// </summary>
    [XmlType(nameof(SimplifiedFlexDirection), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedFlexDirection
    {
        /// <summary>
        /// Horizontal stacking (left to right)
        /// </summary>
        [XmlEnum("horizontal")]
        Horizontal,
        /// <summary>
        /// Vertical stacking (top to bottom)
        /// </summary>
        [XmlEnum("vertical")]
        Vertical
    }

    /// <summary>
    /// Simplified flex justify
    /// </summary>
    [XmlType(nameof(SimplifiedFlexAlignment), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedFlexAlignment
    {
        /// <summary>
        /// Stacking of elements occurs to the left
        /// </summary>
        [XmlEnum("left")]
        Left,
        /// <summary>
        /// Stacking of elements occurs in the center
        /// </summary>
        [XmlEnum("center")]
        Center,
        /// <summary>
        /// Stacking of elements occurs to the right
        /// </summary>
        [XmlEnum("right")]
        Right,
        /// <summary>
        /// Items should be justified with equal space between
        /// </summary>
        [XmlEnum("between")]
        Between
    }

    /// <summary>
    /// Flex layout - The objects in this layout will be stacked horizontally or vertically
    /// </summary>
    [XmlType(nameof(SimplifiedViewFlexLayout), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewFlexLayout : SimplifiedViewComponentCollection, ISimplifiedViewLayout
    {

        /// <summary>
        /// Directou of the stacking of this flex 
        /// </summary>
        [XmlAttribute("direction"), JsonProperty("direction")]
        public SimplifiedFlexDirection Direction { get; set; }

        /// <summary>
        /// Alignment of items which are stacked in this layout
        /// </summary>
        [XmlAttribute("align"), JsonProperty("align")]
        public SimplifiedFlexAlignment Alignment { get; set; }

        /// <inheritdoc/>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            var cssClasses = string.Empty;
            switch(this.Direction)
            {
                case SimplifiedFlexDirection.Horizontal:
                    cssClasses = "d-flex flex-row";
                    switch(this.Alignment)
                    {
                        case SimplifiedFlexAlignment.Left:
                            cssClasses += " justify-content-start";
                            break;
                        case SimplifiedFlexAlignment.Right:
                            cssClasses += " justify-content-end";
                            break;
                        case SimplifiedFlexAlignment.Center:
                            cssClasses += " justify-content-center";
                            break;
                        case SimplifiedFlexAlignment.Between:
                            cssClasses += " justify-content-between";
                            break;
                    }
                    break;
                case SimplifiedFlexDirection.Vertical:
                    cssClasses = "d-flex flex-column";
                    break;
            }
            return $"{cssClasses} {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            base.Render(renderContext);
            renderContext.HtmlWriter.WriteEndElement(); // div
        }
    }
}