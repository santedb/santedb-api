using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Convert to binary data
    /// </summary>
    public class ToBinaryTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "ToBinary";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if(input is String str)
            {
                return Encoding.UTF8.GetBytes(str);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(input), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(string), input.GetType()));
            }
        }
    }
}
