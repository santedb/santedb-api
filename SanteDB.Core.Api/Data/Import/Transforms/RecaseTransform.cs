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
using System.Text.RegularExpressions;
using System.Linq;
using SanteDB.Core.Security.Services;
using System;
using SanteDB.Core.i18n;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Camel case transformation
    /// </summary>
    public class RecaseTransform : IForeignDataElementTransform
    {

        private readonly Regex m_wordRegex = new Regex(@"([A-Za-z0-9])([A-Za-z0-9]*)", RegexOptions.Compiled);

        /// <inheritdoc/>
        public string Name => "Recase";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, System.Collections.Generic.IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if(args.Length == 1)
            {
                var matches = m_wordRegex.Matches(input.ToString()).OfType<Match>();
                switch(args[0].ToString().ToLowerInvariant())
                {
                    case "pascal":
                    case "pascalcase":
                        return String.Join("",matches.Select(o => o.Groups[1].Value.ToUpperInvariant() + o.Groups[2].Value.ToLowerInvariant()));
                    case "camel":
                    case "camelcase":
                        return String.Join("", matches.FirstOrDefault().Groups[1].Value.ToLowerInvariant() + matches.FirstOrDefault().Groups[2].Value.ToLowerInvariant() +
                            matches.Skip(1).Select(o => o.Groups[1].Value.ToUpperInvariant() + o.Groups[2].Value.ToLowerInvariant()));
                    case "upper":
                    case "uppercase":
                        return input?.ToString().ToUpperInvariant();
                    case "lower":
                    case "lowercase":
                        return input?.ToString().ToLowerInvariant();
                    case "word":
                        return String.Join(" ", matches.Select(o => o.Groups[1].Value.ToUpperInvariant() + o.Groups[2].Value));
                    default:
                        throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_OUT_OF_RANGE, args[0], "pascal, camel, upper, lower, word"));
                }
            }
            else
            {
                throw new ArgumentNullException(ErrorMessages.ARGUMENT_NULL);
            }
        }
    }
}
