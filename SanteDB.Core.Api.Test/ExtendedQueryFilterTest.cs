using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Query.FilterExtension;
using SanteDB.Core.Model.Roles;

namespace SanteDB.Core.Api.Test
{
    [TestClass]
    public class ExtendedQueryFilterTest
    {
        [TestMethod]
        public void TestAgeFilter()
        {
            QueryFilterExtensions.AddExtendedFilter(new AgeQueryFilterExtension());
            var filterQuery = NameValueCollection.ParseQueryString("dateOfBirth=:(age)<P3Y");
            var expr = QueryExpressionParser.BuildLinqExpression<Patient>(filterQuery);
            Assert.IsTrue(expr.ToString().Contains("Age"));
        }

        [TestMethod]
        public void TestAgeFilterEx()
        {
            QueryFilterExtensions.AddExtendedFilter(new AgeQueryFilterExtension());
            var filterQuery = NameValueCollection.ParseQueryString("dateOfBirth=:(age|2020-01-01)<P3Y");
            var expr = QueryExpressionParser.BuildLinqExpression<Patient>(filterQuery);
            Assert.IsTrue(expr.ToString().Contains("Age"));

            // Parse it back
            var nvc = QueryExpressionBuilder.BuildQuery(expr);

        }

    }
}
