﻿using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.LZMA;
using SharpCompress.IO;
using SharpCompress.Readers.Tar;
using SharpCompress.Writers.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// A backup writer
    /// </summary>
    public class BackupWriter : IDisposable
    {
        // Magic bytes - how we know a file is a backup
        private static readonly byte[] MAGIC = { (byte)'S', (byte)'D', (byte)'B', (byte)3 };
        private readonly Stream m_underlyingStream;
        private TarWriter m_tarWriter;

        /// <summary>
        /// Create a new backup writer
        /// </summary>
        private BackupWriter(Stream underlyingStream)
        {
            this.m_underlyingStream = underlyingStream;
            this.m_tarWriter = new TarWriter(underlyingStream, new TarWriterOptions(SharpCompress.Common.CompressionType.None, true));
        }

        /// <summary>
        /// Create the backup writer
        /// </summary>
        /// <param name="assetsToWrite">The assets which should be written to the backup file</param>
        /// <param name="password">The password for the backup file</param>
        /// <param name="underlyingStream">The stream to write the backup to</param>
        /// <returns>The backup writer</returns>
        public static BackupWriter Create(Stream underlyingStream, ICollection<IBackupAsset> assetsToWrite, String password = null)
        {

            underlyingStream = new BZip2Stream(NonDisposingStream.Create( underlyingStream), CompressionMode.Compress, false);
            underlyingStream.Write(MAGIC, 0, MAGIC.Length); // emit the magical bytes
            underlyingStream.Write(BitConverter.GetBytes(DateTime.UtcNow.Ticks), 0, sizeof(long));
            underlyingStream.Write(BitConverter.GetBytes((long)assetsToWrite.Count), 0, sizeof(long));
            foreach(var ast in assetsToWrite)
            {
                var data = new BackupAssetInfo(ast).ToByteArray();
                underlyingStream.Write(data, 0, data.Length);
            }

            // Are we encrypting?
            if(String.IsNullOrEmpty(password))
            {
                underlyingStream.Write(Enumerable.Range(0, 16).Select(o => (byte)0).ToArray(), 0, 16);
            }
            else
            {
                // We switch over to the backup stream 
                var desCrypto = AesCryptoServiceProvider.Create();
                var passKey = ASCIIEncoding.ASCII.GetBytes(password);
                passKey = Enumerable.Range(0, 32).Select(o => passKey.Length > o ? passKey[o] : (byte)0).ToArray();
                desCrypto.GenerateIV();
                desCrypto.Key = passKey;
                desCrypto.Padding = PaddingMode.PKCS7;
                underlyingStream.Write(desCrypto.IV, 0, desCrypto.IV.Length);
                underlyingStream = new CryptoStream(underlyingStream, desCrypto.CreateEncryptor(), CryptoStreamMode.Write);
                underlyingStream.Write(MAGIC, 0, MAGIC.Length);
            }

            return new BackupWriter(underlyingStream);
        }

        /// <summary>
        /// Write the asset entry <paramref name="asset"/> to the file
        /// </summary>
        /// <param name="asset">The backup asset to write</param>
        public void WriteAssetEntry(IBackupAsset asset)
        {
            if(this.m_tarWriter == null)
            {
                throw new ObjectDisposedException(nameof(BackupWriter));
            }

            using(var assetStream = asset.Open())
            {
                this.m_tarWriter.Write($"{asset.AssetClassId}/{asset.Name}", assetStream, DateTime.Now);
            }
        }

        /// <summary>
        /// Dispose the writer
        /// </summary>
        public void Dispose()
        {
            if(this.m_tarWriter != null)
            {
                this.m_underlyingStream.Flush();
                this.m_tarWriter.Dispose();
                this.m_underlyingStream.Dispose();
                this.m_tarWriter = null;
            }
        }
    }
}