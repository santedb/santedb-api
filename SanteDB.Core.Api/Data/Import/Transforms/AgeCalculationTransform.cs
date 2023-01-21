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
           
            if (input is int intData || int.TryParse(input.ToString(), out intData)) {

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
