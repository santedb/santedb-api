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
using Newtonsoft.Json;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Simplified row component size
    /// </summary>
    [XmlType(nameof(SimplifiedRowComponentSize), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedRowComponentSize
    {
        /// <summary>
        /// Small - 
        /// XS - 1 per row
        /// MD = 2 per row
        /// LG = 3 per row
        /// XL = 6 per row
        /// </summary>
        [XmlEnum("sm")]
        Small,
        /// <summary>
        /// Medium 
        /// XS - 1 per row
        /// LG - 2 per row
        /// XL - 3 per row
        /// </summary>
        [XmlEnum("md")]
        Medium,
        /// <summary>
        /// Large
        /// LG - 1 per row
        /// XL - 2 per row
        /// </summary>
        [XmlEnum("lg")]
        Large,
        /// <summary>
        /// Extra Large 
        /// XL - 1 per row
        /// </summary>
        [XmlEnum("xl")]
        ExtraLarge
    }
    /// <summary>
    /// Simplified component which belongs in a row
    /// </summary>
    [XmlType(nameof(SimplifiedViewContentComponent), Namespace = "http://santedb.org/model/template/view")]
    public abstract class SimplifiedViewContentComponent : SimplifiedViewComponent
    {
        /// <summary>
        /// The size of this content in the layout within which it belongs
        /// </summary>
        [XmlAttribute("size"), JsonProperty("size")]
        public SimplifiedRowComponentSize Size { get; set; }

        /// <summary>
        /// Size was specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool SizeSpecified { get; set; }

        /// <summary>
        /// Gets the computed CSS classes for this control
        /// </summary>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            var isInRow = renderContext.Component is SimplifiedViewRow;
            var isInFlex = renderContext.Component is SimplifiedViewFlexLayout;

            var cssClasses = string.Empty;
            if (isInRow)
            {
                if (this.SizeSpecified)
                {
                    switch (this.Size)
                    {
                        case SimplifiedRowComponentSize.ExtraLarge:
                            cssClasses = "col-xl-12";
                            break;
                        case SimplifiedRowComponentSize.Large:
                            cssClasses = "col-lg-12 col-xl-6";
                            break;
                        case SimplifiedRowComponentSize.Medium:
                            cssClasses = "col-xs-12 col-lg-6 col-xl-3";
                            break;
                        case SimplifiedRowComponentSize.Small:
                            cssClasses = "col-xs-12 col-md-6 col-lg-3 col-xl-2";
                            break;
                    }
                }
                else
                {
                    cssClasses = "col-xs-12";
                    if(this is SimplifiedViewLabel)
                    {
                        cssClasses += "col-md-3";
                    }
                    else
                    {
                        cssClasses += "col-md-9";
                    }
                }
            } else if (isInFlex)
            {
                if (this.SizeSpecified && this.Size == SimplifiedRowComponentSize.ExtraLarge)
                {
                    cssClasses += " flex-grow-1";
                }
                cssClasses += "m-1";
            }
            return $"{cssClasses} {base.GetCssClasses(renderContext)}".Trim();
        }
    }
}