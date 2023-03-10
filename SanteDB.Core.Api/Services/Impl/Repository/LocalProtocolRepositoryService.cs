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
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Protocol;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Default protocol repository services
    /// </summary>
    public class LocalProtocolRepositoryService : GenericLocalRepository<Model.Acts.Protocol>, IClinicalProtocolRepositoryService
    {
        /// <inheritdoc/>
        public LocalProtocolRepositoryService(IPolicyEnforcementService policyService, IDataPersistenceService<Model.Acts.Protocol> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistence, privacyService)
        {
        }

        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.DeleteClinicalProtocolConfigurationDefinition;
        /// <inheritdoc/>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition;
        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition;
        /// <inheritdoc/>
        protected override string QueryPolicy => PermissionPolicyIdentifiers.ReadMetadata;
        /// <inheritdoc/>
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadMetadata;


        /// <summary>
        /// Find a clinical protocol
        /// </summary>
        [Obsolete("Use FindProtocol(Expression)", true)]
        public IEnumerable<Model.Acts.Protocol> FindProtocol(Expression<Func<Model.Acts.Protocol, bool>> predicate, int offset, int? count, out int totalResults)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Insert a protocol
        /// </summary>
        public IClinicalProtocol InsertProtocol(IClinicalProtocol protocol)
        {
            this.m_policyService.Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition);
            var data = protocol.GetProtocolData();
            if (this.Find(o => o.Oid == data.Oid).Any())
            {
                return protocol.Load(this.Save(data));
            }
            else
            {
                return protocol.Load(base.Insert(data));
            }
        }

        /// <summary>
        /// Find protocol
        /// </summary>
        public IQueryResultSet<IClinicalProtocol> FindProtocol(String protocolName = null, String protocolOid = null)
        {
            Expression<Func<Model.Acts.Protocol, bool>> expression = o => o.ObsoletionTime == null;
            var expressionBody = expression.Body;
            var expressionParameter = expression.Parameters[0];

            if (!String.IsNullOrEmpty(protocolName))
            {
                expressionBody = Expression.And(Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(expressionParameter, typeof(Model.Acts.Protocol).GetProperty(nameof(Model.Acts.Protocol.Name))),
                    Expression.Constant(protocolName)
                ), expressionBody);
            }
            if (!String.IsNullOrEmpty(protocolOid))
            {
                expressionBody = Expression.And(Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(expressionParameter, typeof(Model.Acts.Protocol).GetProperty(nameof(Model.Acts.Protocol.Oid))),
                    Expression.Constant(protocolOid)
                ), expressionBody);
            }

            expression = Expression.Lambda<Func<Model.Acts.Protocol, bool>>(expressionBody, expressionParameter);

            return new TransformQueryResultSet<Model.Acts.Protocol, IClinicalProtocol>(this.Find(expression), (a) => this.CreatePublicProtocol(a));
        }

        /// <summary>
        /// Get clinical protocol by identifier
        /// </summary>
        public IClinicalProtocol GetProtocol(Guid protocolUuid)
        {
            var protocolData = this.Get(protocolUuid);
            if (protocolData != null)
            {
                return this.CreatePublicProtocol(protocolData);
            }
            else
            {
                throw new KeyNotFoundException($"ClinicalProtocol/{protocolUuid}");
            }
        }

        /// <summary>
        /// Creates a public protocol
        /// </summary>
        private IClinicalProtocol CreatePublicProtocol(Model.Acts.Protocol protocolData)
        {
            var retVal = Activator.CreateInstance(protocolData.HandlerClass) as IClinicalProtocol;
            retVal.Load(protocolData);
            return retVal;
        }

        /// <inheritdoc/>
        public IClinicalProtocol RemoveProtocol(Guid protocolUuid)
        {
            var retVal = this.Delete(protocolUuid);
            return this.CreatePublicProtocol(retVal);
        }
    }
}
