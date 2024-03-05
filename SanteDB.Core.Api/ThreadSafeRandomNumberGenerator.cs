/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using System;
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
