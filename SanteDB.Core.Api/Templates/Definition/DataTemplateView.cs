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
        [XmlEnum("http://santedb.org/model/template/view:svd")]
        svd,
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
        /// Gets or sets the content of the view
        /// </summary>
        [XmlElement("div", Namespace = "http://www.w3.org/1999/xhtml", Type = typeof(XElement)),
            XmlElement("svd", Namespace = "http://santedb.org/model/template/view", Type = typeof(SimplifiedViewDefinition)),
            XmlElement("bin", Namespace = "http://santedb.org/model/template", Type = typeof(byte[])),
            XmlElement("ref", Namespace = "http://santedb.org/model/template", Type = typeof(String)),
            JsonIgnore]
        public object Content { get; set; }


        /// <summary>
        /// Content helper for JSON
        /// </summary>
        [JsonProperty("content"), XmlIgnore]
        public Object ContentJson {
            get
            {
                switch (this.Content)
                {
                    case XElement xe:
                        return xe.ToString();
                    case SimplifiedViewDefinition se:
                        using(var ms = new MemoryStream())
                        {
                            se.Save(ms);
                            return Encoding.UTF8.GetString(ms.ToArray());
                        }
                    case String str:
                        return str;
                    case byte[] bin:
                        return Convert.ToBase64String(bin);
                    default:
                        return null;
                }
            }
            set
            {
                if(this.m_contentChoice.HasValue)
                {
                    switch(this.m_contentChoice.Value)
                    {
                        case DataTemplateContentChoice.bin:
                            if (value is byte[] b)
                            {
                                this.Content = b;
                            }
                            else if(value is String binStr)
                            {
                                this.Content = Convert.FromBase64String(binStr);
                            }
                            break;
                        case DataTemplateContentChoice.div:
                            if(value is XElement xel)
                            {
                                this.Content = xel;
                            }
                            else if(value is String xmlStr)
                            {
                                this.Content = XElement.Parse(xmlStr);
                            }
                            break;
                        case DataTemplateContentChoice.svd:
                            if(value is SimplifiedViewDefinition sdv)
                            {
                                this.Content = sdv;
                            }
                            else if(value is String sdvStr)
                            {
                                this.Content = SimplifiedViewDefinition.Parse(sdvStr);
                            }
                            break;
                        case DataTemplateContentChoice.@ref:
                            this.Content = value.ToString();
                            break;
                    }
                }
                else
                {
                    this.Content = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the content choice element
        /// </summary>
        [XmlIgnore, JsonProperty("contentType")]
        public DataTemplateContentChoice ContentChoice
        {
            get
            {
                if (!this.m_contentChoice.HasValue)
                {
                    switch (this.Content)
                    {
                        case XElement x:
                            this.m_contentChoice = DataTemplateContentChoice.div;
                            break;
                        case SimplifiedViewDefinition s:
                            this.m_contentChoice = DataTemplateContentChoice.svd;
                            break;
                        case byte[] b:
                            this.m_contentChoice = DataTemplateContentChoice.bin;
                            break;
                        case String s:
                            this.m_contentChoice = DataTemplateContentChoice.@ref;
                            break;
                    }
                }
                return this.m_contentChoice.GetValueOrDefault();
            }
            set
            {
                this.m_contentChoice = value;
                switch(value)
                {
                    case DataTemplateContentChoice.bin:
                        if(this.Content is String binStr)
                        {
                            this.Content = Convert.FromBase64String(binStr);
                        }
                        break;
                    case DataTemplateContentChoice.div:
                        if(this.Content is String xmlStr)
                        {
                            this.Content = XElement.Parse(xmlStr);
                        }
                        break;
                    case DataTemplateContentChoice.svd:
                        if(this.Content is String fdlStr)
                        {
                            this.Content = SimplifiedViewDefinition.Parse(fdlStr);
                        }
                        else if(this.Content is XElement fdlXml)
                        {
                            this.Content = SimplifiedViewDefinition.Parse(fdlXml.ToString());
                        }
                        break;
                    case DataTemplateContentChoice.@ref:
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
                case SimplifiedViewDefinition sv:
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