using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Camel case transformation
    /// </summary>
    public class CamelCaseTransform : IForeignDataElementTransform
    {

        private readonly Regex m_wordRegex = new Regex(@"[^a-zA-Z]([A-Z-a-z])", RegexOptions.Compiled);

        /// <inheritdoc/>
        public string Name => "CamelCase";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            return m_wordRegex.Replace(input.ToString(), o => o.Groups[1].Value.ToUpperInvariant());
        }
    }
}
