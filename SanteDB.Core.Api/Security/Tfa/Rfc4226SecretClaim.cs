using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SanteDB.Core.Security.Tfa
{
    /// <summary>
    /// The mode that the <see cref="Rfc4226TfaCodeProvider"/> algorithm will operate in.
    /// </summary>
    public enum Rfc4226Mode
    {
        /// <summary>
        /// Counter based mode with increment on generate. Used for SMS, Mail, where the counter should not be time based.
        /// </summary>
        [EnumMember(Value = "hg")]
        HotpIncrementOnGenerate,
        /// <summary>
        /// Counter based mode with increment on validate. Used for hardware tokens where the user must press to receive a code and is not time based.
        /// </summary>
        [EnumMember(Value = "hv")]
        HotpIncrementOnValidate,
        /// <summary>
        /// Time based mode with a 30 second interval. This is the most common form found in authenticator apps that used a synchronized 
        /// </summary>
        [EnumMember(Value = "t30")]
        TotpThirtySecondInterval,
        [EnumMember(Value = "t60")]
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
        [JsonProperty("s")]
        public byte[] Secret { get; set; }
        /// <summary>
        /// Current counter when mode is <see cref="Rfc4226Mode.HotpIncrementOnGenerate"/> or <see cref="Rfc4226Mode.HotpIncrementOnValidate"/>
        /// </summary>
        [JsonProperty("c")]
        public long Counter { get; set; }
        /// <summary>
        /// The starting value for the generation mechanism. Valid when specifying an offset for a TOTP from Unix time
        /// </summary>
        [JsonProperty("sv")]
        public long StartValue { get; set; }
        /// <summary>
        /// The number of digits that should be used in the method. Valid values are 6, 7, and 8.
        /// </summary>
        [JsonProperty("cl")]
        public int CodeLength { get; set; }
        [JsonProperty("i")]
        public bool Initialized { get; set; }

        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("m")]
        public Rfc4226Mode Mode { get; set; }
    }
}
