/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Query.FilterExtension;
using SanteDB.Core.Model.Roles;

namespace SanteDB.Core.Api.Test
{
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "Core API")]
    public class ExtendedQueryFilterTest
    {
        [Test]
        public void TestAgeFilter()
        {
            QueryFilterExtensions.AddExtendedFilter(new AgeQueryFilterExtension());
            var filterQuery = "dateOfBirth=:(age)<P3Y".ParseQueryString();
            var expr = QueryExpressionParser.BuildLinqExpression<Patient>(filterQuery);
            Assert.IsTrue(expr.ToString().Contains("Age"));
        }

        [Test]
        public void TestAgeFilterEx()
        {
            QueryFilterExtensions.AddExtendedFilter(new AgeQueryFilterExtension());
            var filterQuery = "dateOfBirth=:(age|2020-01-01)<P3Y".ParseQueryString();
            var expr = QueryExpressionParser.BuildLinqExpression<Patient>(filterQuery);
            Assert.IsTrue(expr.ToString().Contains("Age"));

            // Parse it back
            var nvc = QueryExpressionBuilder.BuildQuery(expr);

        }

    }
}
