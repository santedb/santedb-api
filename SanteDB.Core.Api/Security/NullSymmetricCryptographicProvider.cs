using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Security
{

    /// <summary>
    /// Symmetric cryptographic provider which does not encrypt
    /// </summary>
    public class NullSymmetricCryptographicProvider : ISymmetricCryptographicProvider
    {

        /// <inheritdoc/>
        public int IVSize => 16;

        /// <inheritdoc/>
        public string ServiceName => "NULL Symmetric Encryption Scheme";

        /// <inheritdoc/>
        public Stream CreateDecryptingStream(Stream underlyingStream, byte[] key, byte[] iv) => underlyingStream;

        /// <inheritdoc/>
        public Stream CreateEncryptingStream(Stream underlyingStream, byte[] key, byte[] iv) => underlyingStream;

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data, byte[] key, byte[] iv) => data;

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data) => data;

        /// <inheritdoc/>
        public string Decrypt(string data) => data;

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data, byte[] key, byte[] iv) => data;

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data) => data;

        /// <inheritdoc/>
        public string Encrypt(string data) => data;

        /// <inheritdoc/>
        public byte[] GenerateIV() => new byte[16]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <inheritdoc/>
        public byte[] GenerateKey() => new byte[0];

        /// <inheritdoc/>
        public byte[] GetContextKey() => this.GenerateKey();

        /// <inheritdoc/>
        public bool RotateContextKey() => true;
    }
}
