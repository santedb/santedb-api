using NUnit.Framework;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Templates.View;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SanteDB.Core.Api.Test
{
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "Core API")]
    public class DataTemplateDefinitionTests
    {

        [Test]
        public void TestCanEmitSimplifiedViewRow()
        {

            var simplifiedDataView = new SimplifiedViewDefinition()
            {
                LayoutPattern = ViewDefinitionLayoutType.grid,
                Content = new SimplifiedViewGridLayout()
                {
                    Content = new List<SimplifiedViewRow>()
                    {
                        new SimplifiedViewRow()
                        {
                            Content = new List<Object>()
                            {
                                new SimplifiedViewLabel()
                                {
                                    Color = SimplifiedTextColor.Blue,
                                    Style = SimplifiedTextStyle.Bold | SimplifiedTextStyle.Italic,
                                    ForInput = "sampleInput",
                                    Hint = "This is a hint",
                                    Text = "Sample Entry",
                                    Size = SimplifiedRowComponentSize.Medium
                                },
                                new SimplifiedViewSelect()
                                {
                                    Name = "sampleInput",
                                    CdssCallback = true,
                                    Binding = "statusConcept",
                                    Options = new List<SimplifiedViewSelectOption>()
                                    {
                                        new SimplifiedViewSelectOption() { Display = "ACTIVE", Value = StatusKeys.Active.ToString() },
                                        new SimplifiedViewSelectOption() { Display = "NOT ACTIVE", Value = StatusKeys.Inactive.ToString() }
                                    },
                                    Required = true,
                                    Size = SimplifiedRowComponentSize.Medium
                                }
                            }
                        }
                    }
                }
            };

            // Save the view
            using(var ms = new MemoryStream())
            {
                simplifiedDataView.Save(ms);
                var definitionStr = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Render to HTML
            using(var sw = new StringWriter())
            {
                using(var xw = XmlWriter.Create(sw, new XmlWriterSettings()
                {
                    Indent = true
                }))
                {
                    simplifiedDataView.Render(xw);
                }

                var html = sw.ToString();
            }
        }
    }
}
