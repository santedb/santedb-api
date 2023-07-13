using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SanteDB.Core.Services.Impl.CheckDigitAlgorithms
{
    /// <summary>
    /// Check digit algorithm
    /// </summary>
    public class SanteDBMod97Algorithm : ICheckDigitAlgorithm
    {
        /// <inheritdoc/>
        public string Name => "MOD-97 Based Algorithm";

        /// <inheritdoc/>
        public string GenerateCheckDigit(string identifierValue)
        {
            var seed = ("0" + identifierValue)
                .Select(i => int.Parse(i.ToString()))
                .Aggregate((a, b) => ((a + b) * 10) % 97);
            seed *= 10;
            seed %= 97;
            var checkDigit = (97 - seed + 1) % 97;
            return checkDigit.ToString().PadLeft(2, '0');
        }

        /// <inheritdoc/>
        public bool ValidateCheckDigit(string identifierValue, string checkDigit)
        {
            return this.GenerateCheckDigit(identifierValue).Equals(checkDigit);
        }
    }
}
