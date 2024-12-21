using Newtonsoft.Json;
using SanteDB.Core.Templates.View;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.Definition
{

    /// <summary>
    /// The type of view
    /// </summary>
    [XmlType(nameof(DataTemplateViewType), Namespace = "http://santedb.org/model/template")]
    public enum DataTemplateViewType
    {
        /// <summary>
        /// The view is intended for displaying a one line summary
        /// </summary>
        [XmlEnum("summary-view")]
        SummaryView,
        /// <summary>
        /// The view is intended for a detailed view 
        /// </summary>
        [XmlEnum("detail-view")]
        DetailView,
        /// <summary>
        /// The view is inteded for input
        /// </summary>
        [XmlEnum("entry-form")]
        Entry,
        /// <summary>
        /// The view is intended for back-entry
        /// </summary>
        [XmlEnum("back-form")]
        BackEntry
    }

    ///Content choice
    [XmlType(nameof(DataTemplateContentChoice), Namespace = "http://santedb.org/model/template")]
    public enum DataTemplateContentChoice
    {
        [XmlEnum("http://www.w3.org/1999/xhtml:div")]
        div,
        [XmlEnum("http://santedb.org/model/template/view:fdl")]
        fdl,
        [XmlEnum("bin")]
        bin,
        [XmlEnum("ref")]
        @ref
    }

    /// <summary>
    /// Represents a single view for the template
    /// </summary>
    [XmlType(nameof(DataTemplateView), Namespace = "http://santedb.org/model/template")]
    public class DataTemplateView
    {
        // Content choice
        private DataTemplateContentChoice? m_contentChoice;

        /// <summary>
        /// Gets or sets the type of the view
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public DataTemplateViewType ViewType { get; set; }

        /// <summary>
        /// Gets or sets the content choice element
        /// </summary>
        [XmlIgnore, JsonProperty("contentType")]
        public DataTemplateContentChoice ContentChoice
        {
            get
            {
                if(!this.m_contentChoice.HasValue)
                {
                    switch(this.Content)
                    {
                        case XElement x:
                            this.m_contentChoice = DataTemplateContentChoice.div;
                            break;
                        case SimplifiedDataEntryView s:
                            this.m_contentChoice = DataTemplateContentChoice.fdl;
                            break;
                        case byte[] b:
                            this.m_contentChoice = DataTemplateContentChoice.bin;
                            break;
                        case String s:
                            this.m_contentChoice = DataTemplateContentChoice.@ref;
                            break;
                    }
                }
                return this.m_contentChoice.Value;
            }
            set => this.m_contentChoice = value;
        }

        /// <summary>
        /// Gets or sets the content of the view
        /// </summary>
        [XmlElement("div", Namespace = "http://www.w3.org/1999/xhtml", Type = typeof(XElement)),
            XmlElement("fdl", Namespace = "http://santedb.org/model/template/view", Type = typeof(SimplifiedDataEntryView)),
            XmlElement("bin", Namespace = "http://santedb.org/model/template", Type = typeof(byte[])),
            XmlElement("ref", Namespace = "http://santedb.org/model/template", Type = typeof(String)),
            JsonIgnore]
        public object Content { get; set; }

        /// <summary>
        /// Gets or sets the content for JSON serialization
        /// </summary>
        [XmlIgnore, JsonProperty("content")]
        public string ContentJson
        {
            get => this.Content?.ToString();
            set
            {
                // Correct the view types 
                switch (this.ContentChoice)
                {
                    case DataTemplateContentChoice.div:
                        this.Content = XElement.Parse(value);
                        break;
                    case DataTemplateContentChoice.fdl:
                        this.Content = SimplifiedDataEntryView.Parse(value);
                        break;
                    case DataTemplateContentChoice.@ref:
                        this.Content = value;
                        break;
                    case DataTemplateContentChoice.bin:
                        this.Content = Convert.FromBase64String(value);
                        break;
                }
            }
        }

        /// <summary>
        /// Render the content to the <paramref name="toStream"/>
        /// </summary>
        public void Render(Stream toStream)
        {
            switch (this.Content)
            {
                case XElement xe:
                    var buf = Encoding.UTF8.GetBytes(xe.ToString());
                    toStream.Write(buf, 0, buf.Length);
                    break;
                case SimplifiedDataEntryView sv:
                    using (var xw = XmlWriter.Create(toStream, new XmlWriterSettings()
                    {
                        CloseOutput = false,
                        OmitXmlDeclaration = true,
                        Encoding = Encoding.UTF8
                    }))
                    {
                        sv.Render(xw);
                    }
                    break;
                case byte[] b:
                    toStream.Write(b, 0, b.Length);
                    break;
                default:
                    throw new InvalidOperationException("Content is a reference");
            }
        }
    }
}