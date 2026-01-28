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
 * Date: 2023-6-21
 */
using SanteDB.Core.i18n;
using System;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Calculates a date from an age
    /// </summary>
    public class AgeCalculationTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "AgeCalculation";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, System.Collections.Generic.IDictionary<string, string> dataMapParameters, params object[] args)
        {

            if (input is int intData || int.TryParse(input.ToString(), out intData))
            {

                var sourceDate = args.Length == 1 ? sourceRecord[args[0].ToString()] : DateTime.Now;

                switch (sourceDate)
                {
                    case DateTime dt:
                        return dt.AddYears(-intData);
                    case DateTimeOffset dto:
                        return dto.AddYears(-intData);
                    case String str:
                        return DateTime.Parse(str).AddYears(-intData);
                    default:
                        throw new ArgumentOutOfRangeException(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(input), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(int), input.GetType()));
            }
        }
    }
}
