/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * User: fyfej
 * Date: 2023-3-10
 */
using Newtonsoft.Json;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;

namespace SanteDB.Core.Security.Tfa
{
    /// <summary>
    /// An implementation of the <see cref="ITfaCodeProvider"/> that adhers to RFC4226 for HOTP secret generation and also implements RFC 6238 for TOTP.
    /// </summary>
    public class Rfc4226TfaCodeProvider : ITfaCodeProvider, ITfaSecretManager
    {
        readonly IApplicationServiceContext _ServiceContext;
        readonly Tracer _Tracer;

        /// <summary>
        /// Creates a new <see cref="Rfc4226TfaCodeProvider"/>
        /// </summary>
        public Rfc4226TfaCodeProvider(IApplicationServiceContext serviceContext)
        {
            _Tracer = new Tracer(nameof(Rfc4226TfaCodeProvider));
            _ServiceContext = serviceContext;
        }

        /// <inheritdoc/>
        public string GenerateTfaCode(IIdentity identity)
        {
            var secrets = GetSecretsForIdentity(identity)?.Where(s => s.Initialized);

            var selectedsecret = secrets?.FirstOrDefault(s => s.Initialized);

            if (null == selectedsecret)
            {
                throw new ArgumentException("No tfa secrets are registered on the identity.", nameof(identity));
            }

            var counter = GetSecretCounter(selectedsecret);


            if (selectedsecret.Mode == Rfc4226Mode.HotpIncrementOnGenerate)
            {
                selectedsecret.Counter++;
                UpdateSecretsForIdentity(identity, secrets, AuthenticationContext.SystemPrincipal);
            }
            return GenerateCode(counter, selectedsecret.Secret, selectedsecret.CodeLength);
        }

        /// <inheritdoc/>
        public bool VerifyTfaCode(IIdentity identity, string code, DateTimeOffset? timeProvided = null)
        {
            var secrets = GetSecretsForIdentity(identity)?.Where(s => s.Initialized);

            if (secrets?.Count() < 1)
            {
                _Tracer.TraceInfo("VerifyTfaCode called but no initialized secrets exist for the user {0}.", identity.Name);
                return false;
            }

            long counter = 0;

            foreach (var secret in secrets)
            {
                counter = GetSecretCounter(secret);

                if (GenerateCodesForVerification(counter, secret.Secret, secret.CodeLength, 1)?.Contains(code) == true)
                {
                    if (secret.Mode == Rfc4226Mode.HotpIncrementOnValidate)
                    {
                        secret.Counter++;
                        UpdateSecretsForIdentity(identity, secrets, AuthenticationContext.SystemPrincipal);
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the valid codes for a given counter value and key, including valid codes around the counter value defined by the slew.
        /// </summary>
        /// <param name="counter">The counter value that is being compared.</param>
        /// <param name="key">The key to use to generate the codes.</param>
        /// <param name="numberOfDigits">The number of digits the code should be. Valid values are 6, 7, and 8 as defined in RFC 4226.</param>
        /// <param name="counterSlew">The slew in counter values defines teh allowed range of counter values. For example, a <paramref name="counterSlew"/> of 1 will generate 3 codes.</param>
        /// <returns>An enumerable containing the codes that are valid for the counter including any counter values around the counter based on the slew.</returns>
        private static IEnumerable<string> GenerateCodesForVerification(long counter, byte[] key, int numberOfDigits, int counterSlew)
        {
            yield return GenerateCode(counter, key, numberOfDigits);

            for (var slew = 1; slew <= counterSlew; slew++)
            {
                yield return GenerateCode(counter - slew, key, numberOfDigits);
                yield return GenerateCode(counter + slew, key, numberOfDigits);
            }

            yield break;
        }

        private static string GenerateCode(long counter, byte[] key, int numberOfDigits)
        {
            using (var alg = HMACSHA1.Create())
            {
                alg.Key = key;

                var input = BitConverter.GetBytes(counter);

                if (BitConverter.IsLittleEndian)
                {
                    byte t1 = input[0];
                    byte t2 = input[7];
                    input[0] = t2;
                    input[7] = t1;
                    t1 = input[1];
                    t2 = input[6];
                    input[1] = t2;
                    input[6] = t1;
                    t1 = input[2];
                    t2 = input[5];
                    input[2] = t2;
                    input[5] = t1;
                    t1 = input[3];
                    t2 = input[4];
                    input[3] = t2;
                    input[4] = t1;
                }

                var hashtext = alg.ComputeHash(input);

                var offset = hashtext[hashtext.Length - 1] & 0x0F;

                var number = BitConverter.ToUInt32(hashtext, offset);

                if (BitConverter.IsLittleEndian)
                {
                    number = unchecked(((number & 0xFF) << 24) +
                        ((number & 0xFF00) << 8) +
                        ((number & 0xFF0000) >> 8) +
                        ((number & 0xFF000000) >> 24));
                }

                number &= 0x7FFFFFFF; //Drop the sign bit

                switch (numberOfDigits)
                {
                    case 6:
                    case 7:
                    case 8:
                        return (number % Math.Pow(10, numberOfDigits)).ToString();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(numberOfDigits), "Invalid number of digits. Must be 6, 7, or 8");
                }
            }
        }

        private Rfc4226SecretClaim AddSecretClaim(IIdentity identity, Rfc4226SecretClaim claim, IPrincipal principal)
        {
            if (null == identity)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (null == claim)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var identityprovider = _ServiceContext.GetService<IIdentityProviderService>();

            if (null == identityprovider)
            {
                throw new InvalidOperationException("No IIdentityProviderService is present.");
            }

            var sdbclaim = new SanteDBClaim(SanteDBClaimTypes.SanteDBRfc4226Secret, JsonConvert.SerializeObject(claim));

            identityprovider.AddClaim(identity.Name, sdbclaim, principal);

            return claim;
        }


        private List<Rfc4226SecretClaim> GetSecretsForIdentity(IIdentity identity)
        {
            if (null == identity)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            var identityprovider = _ServiceContext.GetService<IIdentityProviderService>();

            if (identityprovider == null)
            {
                throw new InvalidOperationException("No IIdentityProviderService is present.");
            }

            var claims = identityprovider.GetClaims(identity.Name)?.Where(c => c.Type == SanteDBClaimTypes.SanteDBRfc4226Secret);

            var secrets = new List<Rfc4226SecretClaim>();

            if (null != claims)
            {
                foreach (var claim in claims)
                {
                    try
                    {
                        secrets.Add(JsonConvert.DeserializeObject<Rfc4226SecretClaim>(claim.Value));
                    }
                    catch
                    {
                        //TODO: Log this.
                    }
                }


            }

            return secrets;
        }

        private void UpdateSecretsForIdentity(IIdentity identity, IEnumerable<Rfc4226SecretClaim> claims, IPrincipal principal)
        {
            if (null == identity)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            var identityprovider = _ServiceContext.GetService<IIdentityProviderService>();

            if (null == identityprovider)
            {
                throw new InvalidOperationException("No IIdentityProviderService is present.");
            }

            identityprovider.RemoveClaim(identity.Name, SanteDBClaimTypes.SanteDBRfc4226Secret, principal);

            if (claims?.Count() > 0)
            {
                foreach (var claim in claims)
                {
                    identityprovider.AddClaim(identity.Name, new SanteDBClaim(SanteDBClaimTypes.SanteDBRfc4226Secret, JsonConvert.SerializeObject(claim)), principal);
                }
            }
        }

        /// <inheritdoc/>
        public string StartTfaRegistration(IIdentity identity, int codeLength, Rfc4226Mode rfc4226Mode, IPrincipal principal)
        {
            var secret = new Rfc4226SecretClaim();

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var key = new byte[20];
                rng.GetBytes(key);
                secret.Secret = key;
            }

            secret.CodeLength = codeLength;
            secret.Mode = rfc4226Mode;
            //TODO: Make these configurable.
            secret.StartValue = 0;
            secret.Initialized = false;

            var counter = GetSecretCounter(secret);

            AddSecretClaim(identity, secret, principal);

            return GenerateCode(counter, secret.Secret, secret.CodeLength);

        }

        /// <inheritdoc/>
        public bool FinishTfaRegistration(IIdentity identity, string code, IPrincipal principal)
        {
            if (null == identity)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            var secrets = GetSecretsForIdentity(identity);

            var secret = secrets?.SingleOrDefault(s => !s.Initialized);

            if (null == secret)
            {
                throw new ArgumentException("Identity has no Tfa Registrations in progress.", nameof(identity));
            }

            long counter = GetSecretCounter(secret);

            if (GenerateCodesForVerification(counter, secret.Secret, secret.CodeLength, 5)?.Contains(code) == true)
            {
                secret.Initialized = true;

                UpdateSecretsForIdentity(identity, secrets, principal);

                return true;
            }
            else
            {
                return false;
            }

        }

        private static long GetSecretCounter(Rfc4226SecretClaim secret)
        {
            switch (secret.Mode)
            {
                case Rfc4226Mode.HotpIncrementOnGenerate:
                case Rfc4226Mode.HotpIncrementOnValidate:
                    return secret.Counter;
                case Rfc4226Mode.TotpThirtySecondInterval:
                    return (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - secret.StartValue) / 30L;
                case Rfc4226Mode.TotpSixtySecondInterval:
                    return (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - secret.StartValue) / 60L;
                default:
                    throw new ArgumentException("Invalid Rfc4226 Mode.", nameof(secret.Mode));
            }
        }
    }
}
