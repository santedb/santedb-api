/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SanteDB.Core.Api.Security;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a record pointer service using JWS
    /// </summary>
    [ServiceProvider("JWS Resource Pointer", Dependencies = new Type[] { typeof(IDataSigningService) })]
    public class JwsResourcePointerService : IResourcePointerService
    {
        /// <summary>
        /// JWS format regex
        /// </summary>
        private readonly Regex m_jwsFormat = new Regex(@"^(.*?)\.(.*?)\.(.*?)$");

        /// <summary>
        /// Name of the service
        /// </summary>
        public String ServiceName => "JSON Web Signature Resource Pointers";

        /// <summary>
        /// Get the key identifier for the signature
        /// </summary>
        private String GetAppKeyId()
        {
            var signatureService = ApplicationServiceContext.Current.GetService<IDataSigningService>();
            if (signatureService == null)
                throw new InvalidOperationException("Cannot find data signing service");

            // Is there a key called jwsdefault? If so, use it as the default signature key
            if (signatureService.GetKeys().Contains("jwsdefault"))
                return "jwsdefault";

            // Otherwise use the configured secure application HMAC key as the default
            if (AuthenticationContext.Current.Principal is IClaimsPrincipal claimsPrincipal)
            {
                // Is there a tag for their application
                var appId = claimsPrincipal.FindFirst(SanteDBClaimTypes.SanteDBApplicationIdentifierClaim)?.Value;
                if (String.IsNullOrEmpty(appId))
                    throw new InvalidOperationException("Can only generate signed pointers when authenticated");
                var keyId = $"SA.{appId}";

                // Does the key provider have the key for this app?
                if (signatureService.GetKeys().Any(k => k == keyId))
                    return keyId;
                else
                {
                    // Application identity
                    var appIdentity = claimsPrincipal.Identities.OfType<IApplicationIdentity>().First();
                    var key = ApplicationServiceContext.Current.GetService<IApplicationIdentityProviderService>().GetSecureKey(appIdentity.Name);

                    // Get the key 
                    signatureService.AddSigningKey(keyId, key, "HS256");

                    return keyId;
                }
            }
            else
                throw new InvalidOperationException("Cannot generate a personal key without knowing application id");
        }

        /// <summary>
        /// Generate the structured pointer
        /// </summary>
        public string GeneratePointer<TEntity>(IEnumerable<IdentifierBase<TEntity>> identifers) where TEntity : VersionedEntityData<TEntity>, new()
        {
            // Setup signatures
            var signatureService = ApplicationServiceContext.Current.GetService<IDataSigningService>();
            if (signatureService == null)
                throw new InvalidOperationException("Cannot find data signing service");

            var keyId = this.GetAppKeyId();

            var entityType = identifers.FirstOrDefault()?.LoadProperty<Entity>("SourceEntity")?.GetType() ??
                typeof(TEntity);
            // Append the header to the token
            // Append authorities to identifiers
            var header = new
            {
                alg = signatureService.GetSignatureAlgorithm(),
                typ = $"x-santedb+{entityType.GetSerializationName()}",
                key = keyId
            };

            // From RFC7515
            var hdrString = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)).Base64UrlEncode();
            StringBuilder identityToken = new StringBuilder($"{hdrString}.");

            var domainList = new
            {
                iat = DateTime.Now.ToUnixEpoch(),
                id = identifers.Select(o => new
                {
                    value = o.Value,
                    ns = o.LoadProperty<AssigningAuthority>("Authority").DomainName
                }).ToList()
            };

            // From RFC7515
            var bodyString = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainList)).Base64UrlEncode();
            identityToken.Append(bodyString);

            // Sign the data
            // From RFC7515
            var tokenData = Encoding.UTF8.GetBytes(identityToken.ToString());
            var signature = signatureService.SignData(tokenData, keyId);
            identityToken.AppendFormat(".{0}", signature.Base64UrlEncode());

            return identityToken.ToString();
        }

        /// <summary>
        /// Let's resolve the specified resource
        /// </summary>
        public IdentifiedData ResolveResource(string data, bool validate = true)
        {
            try
            {
                var match = this.m_jwsFormat.Match(data);
                if (!match.Success)
                    throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.invalid", "Invalid Barcode Format", DetectedIssueKeys.InvalidDataIssue));

                // Get the parts of the header
                byte[] headerBytes = match.Groups[1].Value.ParseBase64UrlEncode(),
                    bodyBytes = match.Groups[2].Value.ParseBase64UrlEncode(),
                    signatureBytes = match.Groups[3].Value.ParseBase64UrlEncode();

                // Now lets parse the JSON objects
                dynamic header = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(headerBytes));
                dynamic body = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bodyBytes));

                // Now validate the payload
                if (!header.typ.ToString().StartsWith("x-santedb+"))
                    throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.invalid.type", "Invalid Barcode Type", DetectedIssueKeys.InvalidDataIssue));
                var type = new ModelSerializationBinder().BindToType(null, header.typ.ToString().Substring(10));
                var algorithm = header.alg.ToString();
                String keyId = header.key.ToString();

                // Attempt to locate the record
                var domainQuery = new NameValueCollection();
                foreach (var id in body.id)
                    domainQuery.Add($"identifier[{id.ns.ToString()}].value", id.value.ToString());
                var filterExpression = QueryExpressionParser.BuildLinqExpression(type, domainQuery);

                // Get query
                var repoType = typeof(IRepositoryService<>).MakeGenericType(type);
                var repoService = (ApplicationServiceContext.Current as IServiceProvider).GetService(repoType) as IRepositoryService;
                if (repoService == null)
                    throw new InvalidOperationException("Cannot find appropriate repository service");

                // HACK: .NET is using late binding and getting confused
                var results = repoService.Find(filterExpression, 0, 2,out int tr) as IEnumerable<IdentifiedData>;
                if (tr > 1)
                    throw new InvalidOperationException("Resource is ambiguous (points to more than one resource)");
                
                var result = results.FirstOrDefault();

                // Validate the signature if we have the key
                if (validate)
                {
                    // Validate the signature service can service the algorithm
                    var signatureService = ApplicationServiceContext.Current.GetService<IDataSigningService>();

                    // We have the key?
                    if(!signatureService.GetKeys().Any(k=>k == keyId))
                    {
                        // Is this an app key id?
                        if(keyId.StartsWith("SA."))
                        {
                            var appId = Guid.Parse(keyId.Substring(3));
                            var appInstance = ApplicationServiceContext.Current.GetService<IRepositoryService<SecurityApplication>>().Get(appId);
                            if (appInstance == null)
                                throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.app", "Unknown source application", DetectedIssueKeys.SecurityIssue));

                            var secret = ApplicationServiceContext.Current.GetService<IApplicationIdentityProviderService>()?.GetSecureKey(appInstance.Name);
                            signatureService.AddSigningKey(keyId, secret, "HS256");
                        }
                        else
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.key", "Invalid Key Type", DetectedIssueKeys.SecurityIssue));
                    }

                    if (signatureService.GetSignatureAlgorithm(keyId) != algorithm)
                        throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.algorithm", $"Algorithm {algorithm} Not Supported (expected {signatureService.GetSignatureAlgorithm(keyId)})", DetectedIssueKeys.SecurityIssue));

                    var payload = Encoding.UTF8.GetBytes($"{match.Groups[1].Value}.{match.Groups[2].Value}");

                    if (!signatureService.Verify(payload, signatureBytes, keyId))
                        throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.verification", "Barcode Tampered", DetectedIssueKeys.SecurityIssue));
                }
                // Return the result
                return result;

            }
            catch (DetectedIssueException) { throw; }
            catch (Exception e)
            {
                throw new Exception("Cannot resolve QR code", e);
            }
        }

    }
}
