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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Security.Signing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a record pointer service using JWS
    /// </summary>
    [ServiceProvider("JWS Resource Pointer", Dependencies = new Type[] { typeof(IDataSigningService) })]
    public class JwsResourcePointerService : IResourcePointerService
    {
        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(JwsResourcePointerService));

        // Application identity service
        private readonly IApplicationIdentityProviderService m_applicationIdService;

        // Data signing service
        private readonly IDataSigningService m_signingService;

        /// <summary>
        /// DI constructor
        /// </summary>
        public JwsResourcePointerService(IDataSigningService signingService, IApplicationIdentityProviderService applicationIdentityProviderService = null)
        {
            this.m_signingService = signingService;
            this.m_applicationIdService = applicationIdentityProviderService;
        }

        /// <summary>
        /// Name of the service
        /// </summary>
        public String ServiceName => "JSON Web Signature Resource Pointers";

        /// <summary>
        /// Get the key identifier for the signature
        /// </summary>
        private String GetAppKeyId()
        {
            // Is there a key called jwsdefault? If so, use it as the default signature key
            if (m_signingService.GetKeys().Contains("default"))
            {
                return "default";
            }

            // Otherwise use the configured secure application HMAC key as the default
            if (AuthenticationContext.Current.Principal is IClaimsPrincipal claimsPrincipal)
            {
                // Is there a tag for their application
                var appIdentity = claimsPrincipal.Identities.OfType<IApplicationIdentity>().First();

                if (appIdentity == null)
                {
                    throw new InvalidOperationException("Can only generate signed pointers when an application identity exists");
                }

                var keyId = $"SA.{appIdentity.Name}";

                // Does the key provider have the key for this app?
                if (m_signingService.GetKeys().Any(k => k == keyId))
                {
                    return keyId;
                }
                else
                {
                    // Application identity
                    var key = this.m_applicationIdService.GetPublicSigningKey(appIdentity.Name);

                    // Get the key
                    this.m_signingService.AddSigningKey(keyId, key, Security.Configuration.SignatureAlgorithm.HS256);

                    return keyId;
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot generate a personal key without knowing application id");
            }
        }

        /// <summary>
        /// Generate the structured pointer
        /// </summary>
        public string GeneratePointer(IHasIdentifiers entity)
        {
            if(!(entity is IAnnotatedResource identifiedEntity))
            {
                throw new ArgumentException(nameof(entity), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, entity.GetType(), typeof(IAnnotatedResource)));
            }

            // Setup signatures
            var keyId = this.GetAppKeyId();
            var domainList = new
            {
                iat = DateTimeOffset.Now.ToUnixTimeSeconds(),
                eid = identifiedEntity.Key.ToString(),
                id = entity.Identifiers.Select(o => new
                {
                    value = o.Value,
                    ns = o.IdentityDomain.DomainName
                }).ToList()
            };

            return JsonWebSignature.Create(domainList, this.m_signingService)
                .WithCompression(Http.Description.HttpCompressionAlgorithm.Deflate)
                .WithKey(keyId)
                .WithType($"x-santedb+{entity.GetType().GetSerializationName()}")
                .AsSigned().Token;
        }

        /// <summary>
        /// Let's resolve the specified resource
        /// </summary>
        public IHasIdentifiers ResolveResource(string data, bool validate = true)
        {

            try
            {

                var parseResult = JsonWebSignature.TryParse(data, this.m_signingService, out var parsedToken);

                if(validate || parsedToken == null)
                {
                    switch(parseResult)
                    {
                        case JsonWebSignatureParseResult.AlgorithmAndKeyMismatch:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.algorithm", $"Algorithm Not Supported (expected {this.m_signingService.GetSignatureAlgorithm(parsedToken.Header.KeyId)})", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.InvalidFormat:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.format", $"Token is not in a valid format", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.MissingAlgorithm:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.algorithm", $"Token cannot be validated - missing algorithm", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.MissingKeyId:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.nokey", $"Token cannot be validated - missing key identifier", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.SignatureMismatch:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.verification", "Barcode Tampered", DetectedIssueKeys.SecurityIssue));
                    }
                } 
                else if(!parsedToken.Header.Type.StartsWith("x-santedb+"))
                {
                    throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.invalid.type", "Invalid Object Type", DetectedIssueKeys.InvalidDataIssue));
                }

                var type = new ModelSerializationBinder().BindToType(null, parsedToken.Header.Type.Substring(10));
                
                // Attempt to locate the record
                var domainQuery = new NameValueCollection();
                foreach (var id in parsedToken.Payload.id)
                {
                    domainQuery.Add($"identifier[{id.ns.ToString()}].value", id.value.ToString());
                }

                var filterExpression = QueryExpressionParser.BuildLinqExpression(type, domainQuery);

                // Get query
                var repoType = typeof(IRepositoryService<>).MakeGenericType(type);
                var repoService = (ApplicationServiceContext.Current as IServiceProvider).GetService(repoType) as IRepositoryService;
                if (repoService == null)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.SERVICE_NOT_FOUND, repoType));
                }

                // HACK: .NET is using late binding and getting confused
                var results = repoService.Find(filterExpression);
                if (results.Count() != 1)
                {
                    throw new ConstraintException(ErrorMessages.AMBIGUOUS_DATA_REFERENCE);
                }

                return results.FirstOrDefault() as IHasIdentifiers;
            }
            catch (DetectedIssueException) { throw; }
            catch (Exception e)
            {
                throw new Exception("Cannot resolve QR code", e);
            }
        }
    }
}