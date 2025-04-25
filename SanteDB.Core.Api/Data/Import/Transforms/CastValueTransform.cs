/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Text.RegularExpressions;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Cast a value as another value
    /// </summary>
    public class CastValueTransform : IForeignDataElementTransform
    {

        /// <inheritdoc/>
        public string Name => "Cast";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, System.Collections.Generic.IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0].ToString().ToLowerInvariant())
                {
                    case "int":
                    case nameof(Int32):
                        return (Int32)input;
                    case "double":
                    case nameof(Double):
                        return (Double)input;
                    case "bool":
                    case nameof(Boolean):
                        return Boolean.Parse(input?.ToString() ?? "false");
                    case "string":
                    case nameof(String):
                        return input?.ToString();
                    default:
                        throw new ArgumentOutOfRangeException(args[0].ToString());
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }
}
