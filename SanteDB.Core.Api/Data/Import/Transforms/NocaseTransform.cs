using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Nocase transform
    /// </summary>
    public class NocaseTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "NoCase";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            if(args.Length == 0)
            {
                return input.ToString().ToLowerInvariant();
            }
            else
            {
                switch(args[0].ToString().ToLower())
                {
                    case "l":
                        return input.ToString().ToLowerInvariant();
                    case "u":
                        return input.ToString().ToUpperInvariant();
                    default:
                        throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_OUT_OF_RANGE, args[0], "l|u"));
                }
            }
        }
    }
}
