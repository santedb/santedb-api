using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Text style
    /// </summary>
    [XmlType(nameof(SimplifiedTextStyle), Namespace = "http://santedb.org/model/template/view"), Flags]
    public enum SimplifiedTextStyle
    {
        /// <summary>
        /// Font should be bold
        /// </summary>
        [XmlEnum("bold")]
        Bold = 0x1,
        /// <summary>
        /// Font should be italic
        /// </summary>
        [XmlEnum("italic")]
        Italic = 0x2,
        /// <summary>
        /// Font should be underlined
        /// </summary>
        [XmlEnum("underline")]
        Underline = 0x4,
        /// <summary>
        /// Font should be strikethrough
        /// </summary>
        [XmlEnum("strikethrough")]
        Strikethrough = 0x8
    }

    /// <summary>
    /// Simplified text color
    /// </summary>
    [XmlType(nameof(SimplifiedTextColor), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedTextColor
    {
        /// <summary>
        /// The default color as specified in the color theme
        /// </summary>
        [XmlEnum("default")]
        Default,
        /// <summary>
        /// Red (#ff0000)
        /// </summary>
        [XmlEnum("red")]
        Red,
        /// <summary>
        /// Green (#00ff00)
        /// </summary>
        [XmlEnum("green")]
        Green,
        /// <summary>
        /// Blue (#0000ff)
        /// </summary>
        [XmlEnum("blue")]
        Blue,
        /// <summary>
        /// The primary color as specified by the theme 
        /// </summary>
        [XmlEnum("primary")]
        Primary,
        /// <summary>
        /// The secondary color as specified by the theme
        /// </summary>
        [XmlEnum("secondary")]
        Secondary,
        /// <summary>
        /// The success color as specified by the theme (usually green)
        /// </summary>
        [XmlEnum("success")]
        Success,
        /// <summary>
        /// The danger color as specified by the theme (usually red)
        /// </summary>
        [XmlEnum("danger")]
        Danger,
        /// <summary>
        /// The warning color as specified by the theme (usually yellow)
        /// </summary>
        [XmlEnum("warning")]
        Warning,
        /// <summary>
        /// The light accent color as specified by the theme
        /// </summary>
        [XmlEnum("light")]
        Light,
        /// <summary>
        /// The dark accent color as specified by the theme
        /// </summary>
        [XmlEnum("dark")]
        Dark,
        /// <summary>
        /// The informational color as specified by the theme
        /// </summary>
        [XmlEnum("info")]
        Info
    }

    /// <summary>
    /// A simple component
    /// </summary>
    [XmlType(nameof(SimplifiedViewComponent), Namespace = "http://santedb.org/model/template/view")]
    public abstract class SimplifiedViewComponent
    {

        private static readonly IDictionary<SimplifiedTextColor, String> m_colorClassMap = new Dictionary<SimplifiedTextColor, string>()
        {
            { SimplifiedTextColor.Blue, "text-blue" },
            { SimplifiedTextColor.Red, "text-red" },
            { SimplifiedTextColor.Green, "text-green" },
            { SimplifiedTextColor.Info, "text-info" },
            { SimplifiedTextColor.Light, "text-light" },
            { SimplifiedTextColor.Dark, "text-dark" },
            { SimplifiedTextColor.Danger, "text-dabger" },
            { SimplifiedTextColor.Default, "" },
            { SimplifiedTextColor.Primary, "text-primary" },
            { SimplifiedTextColor.Secondary, "text-secondary" },
            { SimplifiedTextColor.Success, "text-success" },
            { SimplifiedTextColor.Warning, "text-warning" }
        };

        private static readonly IDictionary<SimplifiedTextStyle, String> m_styleClassMap = new Dictionary<SimplifiedTextStyle, String>() {
            { SimplifiedTextStyle.Bold, "font-bold" },
            { SimplifiedTextStyle.Italic, "text-italic" },
            { SimplifiedTextStyle.Strikethrough, "text-strikethrough" },
            { SimplifiedTextStyle.Underline, "text-underline"  }
        };

        /// <summary>
        /// Namespace for XHTML
        /// </summary>
        protected const string NS_XHTML = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// The name of this object
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The font style of the object
        /// </summary>
        [XmlAttribute("style"), JsonProperty("style")]
        public SimplifiedTextStyle Style { get; set; }

        /// <summary>
        /// True if the style is specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool StyleSpecified { get; set; }

        /// <summary>
        /// The foreground color which should be used
        /// </summary>
        [XmlAttribute("color"), JsonProperty("color")]
        public SimplifiedTextColor Color { get; set; }

        /// <summary>
        /// True if color was specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool ColorSpecified { get; set; }

        /// <summary>
        /// Extra CSS classes which are to be added to the object
        /// </summary>
        [XmlAttribute("class"), JsonProperty("class")]
        public string ExtraCssClass { get; set; }

        /// <summary>
        /// Render the current control contents to the <paramref name="renderContext"/>
        /// </summary>
        /// <param name="renderContext">The rendering context onto which this component should be rendered</param>
        public abstract void Render(SimplifiedViewRenderContext renderContext);

        /// <summary>
        /// Get the css classes
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            var cssClasses = String.Empty;
            if (m_colorClassMap.TryGetValue(this.Color, out var colorClass))
            {
                cssClasses = colorClass;
            }
            if (this.StyleSpecified)
            {
                foreach (var kv in m_styleClassMap)
                {
                    if (this.Style.HasFlag(kv.Key))
                    {
                        cssClasses += $" {kv.Value}";
                    }
                }
            }
            return $"{cssClasses} {this.ExtraCssClass}".Trim();
        }
    }
}