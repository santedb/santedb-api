using SanteDB.Core.Configuration;
using SanteDB.Core.i18n;
using SharpCompress.Archives.Tar;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.LZMA;
using SharpCompress.IO;
using SharpCompress.Readers.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represetns a series of <see cref="IBackupAsset"/> instances 
    /// stored in a backup collection
    /// </summary>
    public class BackupReader : IDisposable
    {
        // Magic bytes - how we know a file is a backup
        private static readonly byte[] MAGIC = { (byte)'S', (byte)'D', (byte)'B', (byte)3 };
        private readonly Stream m_underlyingStream;
        private TarReader m_tarReader;

        /// <summary>
        /// Backup manifest
        /// </summary>
        private BackupReader(Stream underlyingStream, DateTime backupDate, BackupAssetInfo[] assets)
        {
            this.m_underlyingStream = underlyingStream;
            this.m_tarReader = TarReader.Open(underlyingStream) ;
            this.BackupAsset = assets;
            this.BackupDate = backupDate;
        }

        /// <summary>
        /// Gets or set sthe backup date
        /// </summary>
        public DateTime BackupDate { get;  }

        /// <summary>
        /// The assets in this backup
        /// </summary>
        public BackupAssetInfo[] BackupAsset { get; }

        /// <summary>
        /// Load the specified 
        /// </summary>
        /// <param name="backupStream">The stream from which the backup should be loaded</param>
        public static BackupReader Open(Stream backupStream, String password = null)
        {

            backupStream = new BZip2Stream(NonDisposingStream.Create(backupStream), SharpCompress.Compressors.CompressionMode.Decompress, false);
            
            // Validate format
            byte[] magicHeader = new byte[MAGIC.Length];
            if (backupStream.Read(magicHeader, 0, magicHeader.Length) != magicHeader.Length ||
                !magicHeader.SequenceEqual(MAGIC))
            {
                throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
            }

            // Next bytes are date
            byte[] longBuffer = new byte[sizeof(long)];
            if (backupStream.Read(longBuffer, 0, longBuffer.Length) != longBuffer.Length)
            {
                throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
            }
            var backupDate = new DateTime(BitConverter.ToInt64(longBuffer, 0));

            // Read the number of asset manifests and populate their data
            if (backupStream.Read(longBuffer, 0, longBuffer.Length) != longBuffer.Length)
            {
                throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
            }
            var backupAsset = new BackupAssetInfo[BitConverter.ToInt64(longBuffer, 0)];

            // Each backup descriptor is 272 bytes
            var assetBuffer = new byte[272];
            for (int ast = 0; ast < backupAsset.Length; ast++)
            {
                if (backupStream.Read(assetBuffer, 0, assetBuffer.Length) != assetBuffer.Length)
                {
                    throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
                }
                backupAsset[ast] = new BackupAssetInfo(assetBuffer);
            }

            // Next byte is the IV (if encrypted)
            var iv = new byte[16];
            if (backupStream.Read(iv, 0, iv.Length) != iv.Length)
            {
                throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
            }

            if (!iv.All(o => o == 0))
            {
                if (password == null)
                {
                    throw new BackupException(ErrorMessages.FILE_ENCRYPTED);
                }

                // We switch over to the backup stream 
                var desCrypto = AesCryptoServiceProvider.Create();
                var passKey = ASCIIEncoding.ASCII.GetBytes(password);
                passKey = Enumerable.Range(0, 32).Select(o => passKey.Length > o ? passKey[o] : (byte)0).ToArray();
                desCrypto.IV = iv;
                desCrypto.Key = passKey;
                desCrypto.Padding = PaddingMode.PKCS7;
                backupStream = new CryptoStream(backupStream, desCrypto.CreateDecryptor(), CryptoStreamMode.Read);
                if(backupStream.Read(magicHeader, 0, MAGIC.Length) != MAGIC.Length)
                {
                    throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
                }
                else if(!MAGIC.SequenceEqual(magicHeader))
                {
                    throw new BackupException(ErrorMessages.FILE_ENCRYPTED_INVALID_PASSPHRASE);

                }

            }

            return new BackupReader(backupStream, backupDate, backupAsset);
        }

       /// <summary>
       /// Get the next entry
       /// </summary>
       /// <param name="assetInfo">The asset information metadata</param>
       /// <returns>The next entry stream</returns>
        public bool GetNextEntry(out IBackupAsset assetInfo)
        {
            if(this.m_tarReader == null)
            {
                throw new ObjectDisposedException(nameof(BackupReader));
            }
            else if(!this.m_tarReader.MoveToNextEntry())
            {
                assetInfo = null;
                return false;
            }
            var assetInfoMeta = this.BackupAsset.First(o => $"{o.AssetClassId}/{o.AssetName}" == this.m_tarReader.Entry.Key);
            assetInfo = new TarBackupAsset(assetInfoMeta.AssetName, assetInfoMeta.AssetClassId, this.m_tarReader.OpenEntryStream());
            return true;
        }

        /// <summary>
        /// Dispose the backup reader
        /// </summary>
        public void Dispose()
        {
            if(this.m_tarReader != null)
            {
                while (this.m_tarReader.MoveToNextEntry()) ; // exhaust the readers if the user did not
                while (this.m_underlyingStream.Read(new byte[1024], 0, 1024) > 0) ;
                this.m_tarReader.Dispose();
                this.m_underlyingStream.Dispose();
                this.m_tarReader = null;
            }
        }
    }

}
