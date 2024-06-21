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
