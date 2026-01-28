/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-6-21
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class TrimTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Trim";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            return input.ToString().Trim();
        }
    }

    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class LeftTrimTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Left";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if(args.Length == 1)
            {
                var tlength = (int)args[0];
                var inStr = input.ToString();
                if(tlength > inStr.Length)
                {
                    tlength = inStr.Length;
                }
                return input.ToString().Substring(0, tlength);
            }
            else
            {
                return input.ToString().Trim();
            }
        }
    }

    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class RightTrimTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Right";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if (args.Length == 1)
            {
                var tlength = (int)args[0];
                var inStr = input.ToString();
                if (tlength > inStr.Length)
                {
                    tlength = inStr.Length;
                }
                return input.ToString().Substring(inStr.Length - tlength);
            }
            else
            {
                return input.ToString().Trim();
            }
        }
    }


    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class SubstringTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "SubString";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if (args.Length > 0)
            {
                var inStr = input.ToString();
                var start = (int)args[0];
                var length = args.Length > 1 ? (int)args[1] : inStr.Length - start;
                return input.ToString().Substring(start, length);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }

}
