using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Text;

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
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            if(args.Length != 1)
            {
                throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_COUNT_MISMATCH, 1, args.Length));
            }

            if (input is int intData || int.TryParse(input.ToString(), out intData)) {
                switch (sourceRecord[args[0].ToString()])
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
