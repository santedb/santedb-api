using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class LeftTrimTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Left";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if(args.Length == 1)
            {
                var tlength = (int)args[0];
                var inStr = input.ToString();
                if(tlength > inStr.Length)
                {
                    tlength = inStr.Length;
                }
                return input.ToString().Substring(0, tlength);
            }
            else
            {
                return input.ToString().Trim();
            }
        }
    }

    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class RightTrimTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Right";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if (args.Length == 1)
            {
                var tlength = (int)args[0];
                var inStr = input.ToString();
                if (tlength > inStr.Length)
                {
                    tlength = inStr.Length;
                }
                return input.ToString().Substring(inStr.Length - tlength);
            }
            else
            {
                return input.ToString().Trim();
            }
        }
    }


    /// <summary>
    /// Trim transform for left trim
    /// </summary>
    public class SubstringTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "SubString";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, IDictionary<string, string> dataMapParameters, params object[] args)
        {
            if (args.Length > 0)
            {
                var inStr = input.ToString();
                var start = (int)args[0];
                var length = args.Length > 1 ? (int)args[1] : inStr.Length - start;
                return input.ToString().Substring(start, length);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }

}
