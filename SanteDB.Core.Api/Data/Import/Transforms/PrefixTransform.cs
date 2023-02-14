using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Prefix transform
    /// </summary>
    public class PrefixTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Prefix";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            if(args.Length == 1)
            {
                return $"{args[0]}{input}";
            }
            else
            {
                throw new ArgumentException("arg1", ErrorMessages.ARGUMENT_NULL);
            }
        }
    }
}
