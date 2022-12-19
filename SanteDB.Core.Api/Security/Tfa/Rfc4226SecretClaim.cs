using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Tfa
{
    public enum Rfc4226Mode
    {
        HotpIncrementOnGenerate,
        HotpIncrementOnValidate,
        TotpThirtySecondInterval,
        TotpSixtySecondInterval
    }

    /// <summary>
    /// A complex claim value to serialize a secret for Two-Factor Authentication.
    /// </summary>
    public class Rfc4226SecretClaim
    {
        /// <summary>
        /// The secret key for the TFA value.
        /// </summary>
        public byte[] Secret { get; set; }
        /// <summary>
        /// Current counter when mode is <see cref="Rfc4226Mode.HotpIncrementOnGenerate"/> or <see cref="Rfc4226Mode.HotpIncrementOnValidate"/>
        /// </summary>
        public long Counter { get; set; }
        /// <summary>
        /// The starting value for the generation mechanism. Valid when specifying an offset for a TOTP from Unix time
        /// </summary>
        public long StartValue { get; set; }
        /// <summary>
        /// The number of digits that should be used in the method. Valid values are 6, 7, and 8.
        /// </summary>
        public int CodeLength { get; set; }

        public bool Initialized { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Rfc4226Mode Mode { get; set; }
    }
}
