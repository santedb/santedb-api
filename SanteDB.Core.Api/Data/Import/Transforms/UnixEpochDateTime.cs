using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Text;

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
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            // Default to full precision
            if(args.Length == 0)
            {
                args = new object[] { "s" };
            }

            if(input is long longInput)
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
