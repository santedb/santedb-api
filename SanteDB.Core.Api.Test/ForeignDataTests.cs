/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using NUnit.Framework;
using SanteDB.Core.Data.Import;
using SanteDB.Core.Services;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{
    /// <summary>
    /// Foreign data tests
    /// </summary>
    [TestFixture]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ForeignDataTests
    {

        [OneTimeSetUp]
        public void Initialize()
        {
            TestApplicationContext.TestAssembly = typeof(JwsTest).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);
            var serviceManager = ApplicationServiceContext.Current.GetService<IServiceManager>();
            serviceManager.AddServiceProvider(typeof(DefaultForeignDataImporter));
        }

        [Test]
        public void TestCanWriteCSV()
        {
            Assert.IsTrue(ForeignDataImportUtil.Current.TryGetDataFormat("csv", out var dataFormat));
            Assert.IsNotNull(dataFormat);
            byte[] csvData = null;
            using (var stream = new MemoryStream())
            {
                using (var dataFile = dataFormat.Open(stream))
                {
                    using (var writer = dataFile.CreateWriter())
                    {
                        var record = new GenericForeignDataRecord(new string[] { "Name", "Gender", "DOB", "Age", "Weight" });
                        record["Name"] = "Bob Smith";
                        record["Gender"] = "M";
                        record["DOB"] = DateTime.Now.Date.AddDays(-10);
                        record["Age"] = new TimeSpan(10, 0, 0, 0);
                        record["Weight"] = 4.33;
                        writer.WriteRecord(record);

                        // We want to change the values with some nulls to ensure it emits "" 
                        record["Name"] = "Jenny Smith";
                        record["Gender"] = "F";
                        record["DOB"] = null;
                        record["Age"] = new TimeSpan(390, 0, 0, 0);
                        record["Weight"] = 67.9;
                        writer.WriteRecord(record);

                        // Ensure that different column ordering is emitted in the same way
                        record = new GenericForeignDataRecord(new string[] { "Name", "Weight", "Age", "DOB", "Gender" });
                        record["Name"] = "Switchy Jones";
                        record["Gender"] = null;
                        record["DOB"] = null;
                        record["Age"] = null;
                        record["Weight"] = 99.3;
                        writer.WriteRecord(record);

                        // Ensure that missing column names are emitted as null
                        record = new GenericForeignDataRecord(new string[] { "Name", "DOB", "Gender" });
                        record["Name"] = "Scooby Doo";
                        record["Gender"] = "M";
                        record["DOB"] = DateTime.Now.Date.AddDays(-30);
                        writer.WriteRecord(record);

                    }
                }

                csvData = stream.ToArray();
            }

            // Test reading back
            using (var stream = new MemoryStream(csvData))
            {
                using (var dataFile = dataFormat.Open(stream))
                {
                    using (var reader = dataFile.CreateReader())
                    {
                        Assert.IsTrue(reader.MoveNext());
                        Assert.AreEqual(5, reader.ColumnCount);
                        Assert.AreEqual("Bob Smith", reader["Name"]);
                        Assert.AreEqual(new TimeSpan(10, 0, 0, 0), reader["Age"]);
                        Assert.IsTrue(reader.MoveNext());
                        Assert.AreEqual("Jenny Smith", reader["Name"]);
                        Assert.AreEqual(67.9, reader["Weight"]);
                        Assert.IsNull(reader["DOB"]);
                        Assert.IsTrue(reader.MoveNext());
                        Assert.AreEqual("Switchy Jones", reader["Name"]);
                        Assert.IsNull(reader["Gender"]);
                        Assert.AreEqual(99.3, reader["Weight"]);
                        Assert.IsTrue(reader.MoveNext());
                        Assert.AreEqual("Scooby Doo", reader["Name"]);
                        Assert.AreEqual(DateTime.Now.Date.AddDays(-30), reader["DOB"]);
                        Assert.IsFalse(reader.MoveNext());
                    }
                }
            }

        }

        [Test]
        public void TestCanReadCSV()
        {
            Assert.IsTrue(ForeignDataImportUtil.Current.TryGetDataFormat("csv", out var dataFormat));
            Assert.IsNotNull(dataFormat);
            using (var stream = typeof(ForeignDataTests).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.Patients.csv"))
            {
                Assert.IsNotNull(stream);
                using (var dataFile = dataFormat.Open(stream))
                {
                    Assert.IsNotNull(dataFile);
                    Assert.Throws<ArgumentOutOfRangeException>(() => dataFile.CreateReader("foo")); // CSV doesn't support tables or worksheets
                    Assert.Throws<ArgumentOutOfRangeException>(() => dataFile.CreateWriter("foo")); // CSV doesn't support tables or worksheets
                    using (var reader = dataFile.CreateReader())
                    {
                        Assert.IsNotNull(reader);
                        Assert.IsTrue(reader.MoveNext());
                        Assert.AreEqual(17, reader.ColumnCount);
                        Assert.AreEqual("MRN", reader.GetName(0));
                        Assert.AreEqual("Mother Family", reader.GetName(14));
                        Assert.AreEqual("3996173413", reader["MRN"]);
                        Assert.AreEqual("993556-002228-1986R", reader["Insurance"]);
                        Assert.IsTrue(reader.MoveNext()); // This column is unescaped mrn so it should be a number
                        Assert.AreEqual(1601850032, reader["MRN"]);
                    }
                }
            }
        }
    }
}
