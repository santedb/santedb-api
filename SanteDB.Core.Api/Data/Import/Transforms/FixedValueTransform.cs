using SanteDB.Core.Model.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Fixed value transformation
    /// </summary>
    public class FixedValueTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "FixedValue";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord dataRecord, params object[] args)
        {
            if(args.Length == 0)
            {
                throw new ArgumentNullException(nameof(args));
            }
            else
            {
                if(MapUtil.TryConvert(args[0], input.GetType(), out var result))
                {
                    return result;
                }
                else
                {
                    return args[0];
                }
            }
        }
    }
}
