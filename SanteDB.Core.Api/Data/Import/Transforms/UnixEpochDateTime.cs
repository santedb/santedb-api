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
 */
using SanteDB.Core.i18n;
using System;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Converts a unix epoch to a date time
    /// </summary>
    public class UnixEpochDateTime : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "EpochToDate";

        /// <inheritdoc/>
        /// <remarks>
        /// The args passed into the parameter indicate the epoch date trunction (day, hour, minute, etc.)
        /// </remarks>
        public object Transform(object input, IForeignDataRecord sourceRecord, System.Collections.Generic.IDictionary<string, string> dataMapParameters, params object[] args)
        {
            // Default to full precision
            if (args.Length == 0)
            {
                args = new object[] { "s" };
            }

            if (input is long longInput)
            {
                if (args[0] is int offset)
                {
                    longInput *= offset;
                }
                else
                {
                    switch (args[0].ToString())
                    {
                        case "y":
                            longInput *= 31557600;
                            break;
                        case "M":
                            longInput *= 2592000;
                            break;
                        case "w":
                            longInput *= 604800;
                            break;
                        case "d":
                            longInput *= 86400;
                            break;
                        case "h":
                            longInput *= 3600;
                            break;
                        case "m":
                            longInput *= 60;
                            break;
                    }
                }
                return DateTimeOffset.FromUnixTimeSeconds(longInput);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(input), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(long), input.GetType()));
            }

        }
    }
}
