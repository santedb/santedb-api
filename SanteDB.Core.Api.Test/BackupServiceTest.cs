﻿using NUnit.Framework;
using SanteDB.Core.Data.Backup;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.Api.Test
{
    /// <summary>
    /// Backup service tests
    /// </summary>
    [TestFixture]
    public class BackupServiceTest 
    {

        /// <summary>
        /// Tests that the BackupReader and BAckupWriter work
        /// </summary>
        [Test]
        public void TestCanBackupRestoreStream()
        {

            var assetClassId = Guid.NewGuid();
            var assets = new IBackupAsset[] { 
                new StreamBackupAsset(assetClassId, "foo", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) ,
                new StreamBackupAsset(assetClassId, "bar", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) ,
                new StreamBackupAsset(assetClassId, "baz", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) ,
                new StreamBackupAsset(assetClassId, "buzz", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) 
            };

            using (var backupStream = new MemoryStream())
            {
                using (var backupWriter = BackupWriter.Create(backupStream, assets))
                {
                    foreach (var ast in assets)
                    {
                        backupWriter.WriteAssetEntry(ast);
                    }
                }
                backupStream.Flush();

                // Now we want to try to read out the data
                backupStream.Seek(0, SeekOrigin.Begin);
                using (var backupReader = BackupReader.Open(backupStream))
                {
                    Assert.AreEqual(assets.Length, backupReader.BackupAsset.Length);
                    int i = 0;
                    while (backupReader.GetNextEntry(out var assetInfo))
                    {
                        using (assetInfo)
                        {
                            Assert.AreEqual(assets[i].AssetClassId, assetInfo.AssetClassId);
                            Assert.AreEqual(assets[i++].Name, assetInfo.Name);
                            var bundle = new XmlSerializer(typeof(Bundle), new Type[] { typeof(EntityMaster<Patient>) }).Deserialize(assetInfo.Open());
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Tests that the BackupReader and BAckupWriter work
        /// </summary>
        [Test]
        public void TestCanBackupRestoreEncryptedStream()
        {

            var assetClassId = Guid.NewGuid();
            var assets = new IBackupAsset[] {
                new StreamBackupAsset(assetClassId, "foo", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) ,
                new StreamBackupAsset(assetClassId, "bar", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) ,
                new StreamBackupAsset(assetClassId, "baz", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml")) ,
                new StreamBackupAsset(assetClassId, "buzz", () => typeof(BackupServiceTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml"))
            };

            using (var backupStream = new MemoryStream())
            {
                using (var backupWriter = BackupWriter.Create(backupStream, assets, "fluffy_fluffy_penguins"))
                {
                    foreach (var ast in assets)
                    {
                        backupWriter.WriteAssetEntry(ast);
                    }
                }
                backupStream.Flush();

                // Now we want to try to read out the data
                backupStream.Seek(0, SeekOrigin.Begin);
                Assert.Throws<BackupException>(() => BackupReader.Open(backupStream));
                backupStream.Seek(0, SeekOrigin.Begin);
                Assert.Throws<BackupException>(() => BackupReader.Open(backupStream, "feathery_feathery_penguins"));
                backupStream.Seek(0, SeekOrigin.Begin);

                using (var backupReader = BackupReader.Open(backupStream, "fluffy_fluffy_penguins"))
                {
                    Assert.AreEqual(assets.Length, backupReader.BackupAsset.Length);
                    int i = 0;
                    while (backupReader.GetNextEntry(out var assetInfo))
                    {
                        using (assetInfo)
                        {
                            Assert.AreEqual(assets[i].AssetClassId, assetInfo.AssetClassId);
                            Assert.AreEqual(assets[i++].Name, assetInfo.Name);
                            var bundle = new XmlSerializer(typeof(Bundle), new Type[] { typeof(EntityMaster<Patient>) }).Deserialize(assetInfo.Open());
                        }
                    }
                }
            }

        }

    }
}