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
 * Date: 2024-12-23
 */
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
