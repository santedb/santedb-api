using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SanteDB.Core
{
    /// <summary>
    /// Thread safe random number generator.
    /// </summary>
    public class ThreadSafeRandomNumberGenerator : Random
    {
        readonly object _Lock;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ThreadSafeRandomNumberGenerator"/>.
        /// </summary>
        public ThreadSafeRandomNumberGenerator()
            : base()
        {
            _Lock = new object();
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="ThreadSafeRandomNumberGenerator"/>.
        /// </summary>
        public ThreadSafeRandomNumberGenerator(int seed) : base(seed)
        {
            _Lock = new object();
        }

        /// <inheritdoc />
        public override int Next()
        {
            return base.Next();
        }

        /// <inheritdoc />
        public override int Next(int maxValue)
        {
            Monitor.Enter(_Lock);
            try
            {
                return base.Next(maxValue);
            }
            finally
            {
                Monitor.Exit(_Lock);
            }
        }

        /// <inheritdoc />
        public override int Next(int minValue, int maxValue)
        {
            Monitor.Enter(_Lock);
            try
            {
                return base.Next(minValue, maxValue);
            }
            finally
            {
                Monitor.Exit(_Lock);
            }
        }

        /// <inheritdoc />
        public override void NextBytes(byte[] buffer)
        {
            Monitor.Enter(_Lock);
            try
            {
                base.NextBytes(buffer);
            }
            finally
            {
                Monitor.Exit(_Lock);
            }
        }

        /// <inheritdoc />
        public override double NextDouble()
        {
            Monitor.Enter(_Lock);
            try
            {
                return base.NextDouble();
            }
            finally
            {
                Monitor.Exit(_Lock);
            }
        }

    }
}
