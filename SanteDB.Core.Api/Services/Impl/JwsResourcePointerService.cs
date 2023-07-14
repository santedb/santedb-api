﻿/*
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
 * Date: 2023-5-19
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Security.Signing;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Security.Principal;

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
        private readonly IDataSigningCertificateManagerService m_dataSigningCertificateManagerService;

        // Data signing service
        private readonly IDataSigningService m_signingService;

        /// <summary>
        /// DI constructor
        /// </summary>
        public JwsResourcePointerService(IDataSigningService signingService, IApplicationIdentityProviderService applicationIdentityProviderService = null, IDataSigningCertificateManagerService dataSigningCertificateManagerService = null)
        {
            this.m_signingService = signingService;
            this.m_applicationIdService = applicationIdentityProviderService;
            this.m_dataSigningCertificateManagerService = dataSigningCertificateManagerService;
        }

        /// <summary>
        /// Name of the service
        /// </summary>
        public String ServiceName => "JSON Web Signature Resource Pointers";

        /// <summary>
        /// Generate the structured pointer
        /// </summary>
        public string GeneratePointer(IHasIdentifiers entity)
        {
            if (!(entity is IAnnotatedResource identifiedEntity))
            {
                throw new ArgumentException(nameof(entity), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, entity.GetType(), typeof(IAnnotatedResource)));
            }

            // Setup signatures
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

            var signature = JsonWebSignature.Create(domainList, this.m_signingService)
                .WithCompression(Http.Description.HttpCompressionAlgorithm.Deflate)
                .WithContentType($"application/x-santedb+{entity.GetType().GetSerializationName()}");

            // Allow a configured identity for SYSTEM (this system's certificate mapping)
            var signingCertificate = this.m_dataSigningCertificateManagerService?.GetSigningCertificates(AuthenticationContext.SystemPrincipal.Identity);
            if(signingCertificate?.Any() == true)
            {
                signature = signature.WithCertificate(signingCertificate.First());
            }
            else
            {
                signature = signature.WithSystemKey("default");
            }

            return signature.AsSigned().Token;
        }

        /// <summary>
        /// Let's resolve the specified resource
        /// </summary>
        public IHasIdentifiers ResolveResource(string data, bool validate = true)
        {

            try
            {

                var parseResult = JsonWebSignature.TryParse(data, this.m_signingService, out var parsedToken);

                if (validate || parsedToken == null)
                {
                    switch (parseResult)
                    {
                        case JsonWebSignatureParseResult.AlgorithmAndKeyMismatch:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.algorithm", $"Algorithm mismatch", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.InvalidFormat:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.format", $"Token is not in a valid format", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.MissingAlgorithm:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.algorithm", $"Token cannot be validated - missing algorithm", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.MissingKeyId:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.nokey", $"Token cannot be validated - missing key identifier", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.SignatureMismatch:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.verification", "Barcode Tampered", DetectedIssueKeys.SecurityIssue));
                        case JsonWebSignatureParseResult.UnsupportedAlgorithm:
                            throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.notsupported", "Unsupported Algorithm", DetectedIssueKeys.SecurityIssue));
                    }
                }
                else if (!parsedToken.Header.Type.StartsWith("application/x-santedb+"))
                {
                    throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "jws.invalid.type", "Invalid Object Type", DetectedIssueKeys.InvalidDataIssue));
                }

                var type = new ModelSerializationBinder().BindToType(null, parsedToken.Header.ContentType.Substring(22));

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