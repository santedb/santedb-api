using Newtonsoft.Json;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Tfa
{
    public class Rfc4226CodeProvider : ITfaCodeProvider, ITfaSecretManager
    {
        readonly IApplicationServiceContext _ServiceContext;

        public Rfc4226CodeProvider(IApplicationServiceContext serviceContext)
        {
            _ServiceContext = serviceContext;
        }

        public string GenerateTfaCode(IIdentity identity, string address = null)
        {
            var secrets = GetSecretsForIdentity(identity)?.Where(s => s.Initialized);

            var selectedsecret = secrets?.FirstOrDefault(s=>address == null || s.Address.Equals(address, StringComparison.OrdinalIgnoreCase));

            var counter = 0L;

            if (selectedsecret.TimeBase != 0) //TOTP
            {
                counter = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - selectedsecret.StartValue) / selectedsecret.TimeBase;
            }
            else //HOTP
            {
                counter = selectedsecret.Counter;
            }

            return GenerateCode(counter, selectedsecret.Secret, selectedsecret.CodeLength);
        }

        public bool VerifyTfaCode(IIdentity identity, string code, DateTimeOffset? timeProvided = null)
        {
            var secrets = GetSecretsForIdentity(identity)?.Where(s=>s.Initialized);

            if (secrets?.Count() < 1)
            {
                //TODO: Log this condition.
                return false;
            }

            long counter = 0;

            foreach(var secret in secrets)
            {
                if (secret.TimeBase != 0)
                {
                    counter = ((timeProvided ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds() - secret.StartValue) / secret.TimeBase;
                }
                else
                {
                    counter = secret.Counter;
                }

                if (GenerateCodesForVerification(counter, secret.Secret, secret.CodeLength, 1)?.Contains(code) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> GenerateCodesForVerification(long counter, byte[] key, int numberOfDigits, int counterSlew)
        {
            yield return GenerateCode(counter, key, numberOfDigits);

            for(var slew = 1; slew <= counterSlew; slew++)
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

        private void UpdateSecretsForIdentity(IIdentity identity, List<Rfc4226SecretClaim> claims, IPrincipal principal)
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

            if (claims?.Count > 0)
            {
                foreach(var claim in claims)
                {
                    identityprovider.AddClaim(identity.Name, new SanteDBClaim(SanteDBClaimTypes.SanteDBRfc4226Secret, JsonConvert.SerializeObject(claim)), principal);
                }
            }
        }

        public string StartTfaRegistration(IIdentity identity, string address, int codeLength, IPrincipal principal)
        {
            var secret = new Rfc4226SecretClaim();

            using(var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var key = new byte[20];
                rng.GetBytes(key);
                secret.Secret = key;
            }

            secret.CodeLength = codeLength;
            secret.Address = address;
            //TODO: Make these configurable.
            secret.StartValue = 0;
            secret.TimeBase = 30;
            secret.Initialized = false;

            var counter = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - secret.StartValue) / secret.TimeBase;

            AddSecretClaim(identity, secret, principal);

            return GenerateCode(counter, secret.Secret, secret.CodeLength);

        }

        public bool FinishTfaRegistration(IIdentity identity, string address, string code, IPrincipal principal)
        {
            if (null == identity)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            var secrets = GetSecretsForIdentity(identity);

            var secret = secrets?.FirstOrDefault(s => s.Address == address);

            if (null == secret)
            {
                throw new ArgumentException("Invalid Address", nameof(address));
            }

            long counter = 0;

            if (secret.TimeBase != 0)
            {
                counter = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - secret.StartValue) / secret.TimeBase;
            }

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
    }
}
