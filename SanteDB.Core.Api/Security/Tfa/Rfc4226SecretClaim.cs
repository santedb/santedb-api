/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

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
        /// <summary>
        /// TOTP has a 60 second interval
        /// </summary>
        [EnumMember(Value = "t60")]
        TotpSixtySecondInterval,
        /// <summary>
        /// Ten minute interval
        /// </summary>
        [EnumMember(Value = "t600")]
        TotpTenMinuteInterval
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
        /// <summary>
        /// Gets or sets whether the initialization has been complete
        /// </summary>
        [JsonProperty("i")]
        public bool Initialized { get; set; }
        /// <summary>
        /// Gets or sets the mode of MFA registration
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("m")]
        public Rfc4226Mode Mode { get; set; }
    }
}
