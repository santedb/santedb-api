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
namespace SanteDB.Core.Security
{
    /// <summary>
    /// Claim types
    /// </summary>
    public static class PermissionPolicyIdentifiers
    {
        #region IMS Policies in the 1.3.6.1.4.1.33349.3.1.5.9.2 namespace

        /// <summary>
        /// Access administrative function
        /// </summary>
        public const string UnrestrictedAll = "1.3.6.1.4.1.33349.3.1.5.9.2";

        /// <summary>
        /// Access administrative function
        /// </summary>
        public const string UnrestrictedAdministration = UnrestrictedAll + ".0";

        /// <summary>
        /// Policy identifier for allowance of changing passwords
        /// </summary>
        public const string ChangePassword = UnrestrictedAdministration + ".1";

        /// <summary>
        /// Whether the user can create roles
        /// </summary>
        public const string CreateRoles = UnrestrictedAdministration + ".2";

        /// <summary>
        /// Policy identifier for allowance of altering passwords
        /// </summary>
        public const string AlterRoles = UnrestrictedAdministration + ".3";

        /// <summary>
        /// Policy identifier for allowing of creating new identities
        /// </summary>
        public const string CreateIdentity = UnrestrictedAdministration + ".4";

        /// <summary>
        /// Policy identifier for allowing of creating new devices
        /// </summary>
        public const string CreateDevice = UnrestrictedAdministration + ".5";

        /// <summary>
        /// Policy identifier for allowing of creating new applications
        /// </summary>
        public const string CreateApplication = UnrestrictedAdministration + ".6";

        /// <summary>
        /// Administer the concept dictionary
        /// </summary>
        public const string AdministerConceptDictionary = UnrestrictedAdministration + ".7";

        /// <summary>
        /// Policy identifier for allowing of creating new identities
        /// </summary>
        public const string AlterIdentity = UnrestrictedAdministration + ".8";

        /// <summary>
        /// Policy identifier for allowing of creating new identities
        /// </summary>
        public const string AlterLocalIdentity = AlterIdentity + ".1";

        /// <summary>
        /// Policy identifier for allowing of creating new identities
        /// </summary>
        public const string CreateLocalIdentity = CreateIdentity + ".1";

        /// <summary>
        /// Allows an identity to alter a policy
        /// </summary>
        public const string AlterPolicy = UnrestrictedAdministration + ".9";

        /// <summary>
        /// Administer data warehouse
        /// </summary>
        public const string AdministerWarehouse = UnrestrictedAdministration + ".10";

        /// <summary>
        /// Unrestricted access to the audit repository
        /// </summary>
        public const string AccessAuditLog = UnrestrictedAdministration + ".11";

        /// <summary>
        /// Access to administrative applet information
        /// </summary>
        public const string AdministerApplet = UnrestrictedAdministration + ".12";

        /// <summary>
        /// Allows identity to alter administrative object policy
        /// </summary>
        public const string AssignPolicy = UnrestrictedAdministration + ".13";

        /// <summary>
        /// Unresitrected pub/sub
        /// </summary>
        public const string UnrestrictedPubSub = UnrestrictedAdministration + ".14";

        /// <summary>
        /// Allow principals to create new subscriptions
        /// </summary>
        public const string CreatePubSubSubscription = UnrestrictedPubSub + ".1";

        /// <summary>
        /// Allow principals to enable pubsub
        /// </summary>
        public const string EnablePubSubSubscription = UnrestrictedPubSub + ".2";

        /// <summary>
        /// Allow principals to delete pubsub subs
        /// </summary>
        public const string DeletePubSubSubscription = UnrestrictedPubSub + ".3";

        /// <summary>
        /// Allow principals to read pubsub subs
        /// </summary>
        public const string ReadPubSubSubscription = UnrestrictedPubSub + ".4";

        /// <summary>
        /// Unrestricted system configuration
        /// </summary>
        public const string AlterSystemConfiguration = UnrestrictedAdministration + ".15";

        /// <summary>
        /// Un-restricted match configurations
        /// </summary>
        public const string UnrestrictedMatchConfiguration = AlterSystemConfiguration + ".1";

        /// <summary>
        /// Un-restricted match configurations
        /// </summary>
        public const string AlterMatchConfiguration = UnrestrictedMatchConfiguration + ".1";

        /// <summary>
        /// Activate match configuration
        /// </summary>
        public const string ActivateMatchConfiguration = UnrestrictedMatchConfiguration + ".2";

        /// <summary>
        /// Unrestricted access to modify clinical protocol definitions
        /// </summary>
        public const string UnrestrictedClinicalProtocolConfiguration = AlterSystemConfiguration + ".2";

        /// <summary>
        /// Create a clinical protocol 
        /// </summary>
        public const string CreateClinicalProtocolConfigurationDefinition = UnrestrictedClinicalProtocolConfiguration + ".0";
        /// <summary>
        /// Unrestricted access to modify clinical protocol definitions
        /// </summary>
        public const string AlterClinicalProtocolConfigurationDefinition = UnrestrictedClinicalProtocolConfiguration + ".1";

        /// <summary>
        /// Delete clinical protocol configurations
        /// </summary>
        public const string DeleteClinicalProtocolConfigurationDefinition = UnrestrictedClinicalProtocolConfiguration + ".2";

        /// <summary>
        /// Manage all dispatcher / persistent queues
        /// </summary>
        public const string ManageDispatcherQueues = UnrestrictedAdministration + ".16";

        /// <summary>
        /// Manage mail
        /// </summary>
        public const string ManageMail = UnrestrictedAdministration + ".17";

        /// <summary>
        /// Unrestricted backups
        /// </summary>
        public const string ManageBackups = UnrestrictedAdministration + ".18";

        /// <summary>
        /// Unrestricted backups
        /// </summary>
        public const string CreateAnyBackup = ManageBackups + ".1";

        /// <summary>
        /// Create Public backups
        /// </summary>
        public const string CreatePrivateBackup = CreateAnyBackup + ".1";

        /// <summary>
        /// Unrestricted certificate management
        /// </summary>
        public const string UnrestrictedCertificate = UnrestrictedAdministration + ".19";

        /// <summary>
        /// Permission to sign and issue certificates
        /// </summary>
        public const string IssueCertificates = UnrestrictedCertificate + ".1";

        /// <summary>
        /// Permission to revoke certificates
        /// </summary>
        public const string RevokeCertificates = UnrestrictedCertificate + ".2";

        /// <summary>
        /// Permission to assign a certificate to an entity
        /// </summary>
        public const string AssignCertificateToIdentity = UnrestrictedCertificate + ".3";

        /// <summary>
        /// Manage foreign data
        /// </summary>
        public const string ManageForeignData = UnrestrictedAdministration + ".20";

        /// <summary>
        /// Unrestricted access to download and view service logs
        /// </summary>
        public const string UnrestrictedServiceLogs = UnrestrictedAdministration + ".21";

        /// <summary>
        /// Read service logs
        /// </summary>
        public const string ReadServiceLogs = UnrestrictedServiceLogs + ".1";

        /// <summary>
        /// Delete service logs
        /// </summary>
        public const string DeleteServiceLogs = UnrestrictedServiceLogs + ".2";

        /// <summary>
        /// Unrestricted access to system jobs
        /// </summary>
        public const string UnrestrictedJobManagement = UnrestrictedAdministration + ".22";

        /// <summary>
        /// View system jobs
        /// </summary>
        public const string ReadSystemJobs = UnrestrictedJobManagement + ".0";

        /// <summary>
        /// Start a system job
        /// </summary>
        public const string StartSystemJob = UnrestrictedJobManagement + ".1";

        /// <summary>
        /// Alter a job schedule
        /// </summary>
        public const string AlterSystemJobSchedule = UnrestrictedJobManagement + ".2";

        /// <summary>
        /// Register a system job
        /// </summary>
        public const string RegisterSystemJob = UnrestrictedJobManagement + ".3";

        /// <summary>
        /// Policy identifier for allowance of login
        /// </summary>
        public const string Login = UnrestrictedAll + ".1";

        /// <summary>
        /// Login to an interactive session (with user interaction)
        /// </summary>
        public const string LoginAsService = Login + ".0";

        /// <summary>
        /// Login for the purposes of password change only
        /// </summary>
        public const string LoginPasswordOnly = LoginAsService + ".1";

        /// <summary>
        /// Allow users to impersonate or use their device credentials
        /// </summary>
        public const string LoginImpersonateApplication = LoginAsService + ".2";

        /// <summary>
        /// Access clinical data permission
        /// </summary>
        public const string UnrestrictedClinicalData = UnrestrictedAll + ".2";

        /// <summary>
        /// Query clinical data
        /// </summary>
        public const string QueryClinicalData = UnrestrictedClinicalData + ".0";

        /// <summary>
        /// Write clinical data
        /// </summary>
        public const string WriteClinicalData = UnrestrictedClinicalData + ".1";

        /// <summary>
        /// Delete clinical data
        /// </summary>
        public const string DeleteClinicalData = UnrestrictedClinicalData + ".2";

        /// <summary>
        /// Read clinical data
        /// </summary>
        public const string ReadClinicalData = UnrestrictedClinicalData + ".3";

        /// <summary>
        /// Allows the exporting of clinical data
        /// </summary>
        public const string ExportClinicalData = UnrestrictedClinicalData + ".4";

        /// <summary>
        /// Indicates the user can elevate themselves (Break the glass)
        /// </summary>
        public const string ElevateClinicalData = UnrestrictedClinicalData + ".5";

        /// <summary>
        /// Indicates the user can update metadata
        /// </summary>
        public const string UnrestrictedMetadata = UnrestrictedAll + ".4";

        /// <summary>
        /// Indicates the user can read metadata
        /// </summary>
        public const string ReadMetadata = UnrestrictedMetadata + ".0";

        /// <summary>
        /// Allow a user all access to the warehouse
        /// </summary>
        public const string UnrestrictedWarehouse = UnrestrictedAll + ".5";

        /// <summary>
        /// Allow a user to write data to the warehouse
        /// </summary>
        public const string WriteWarehouseData = UnrestrictedWarehouse + ".0";

        /// <summary>
        /// Allow a user to write data to the warehouse
        /// </summary>
        public const string DeleteWarehouseData = UnrestrictedWarehouse + ".1";

        /// <summary>
        /// Allow a user to write data to the warehouse
        /// </summary>
        public const string ReadWarehouseData = UnrestrictedWarehouse + ".2";

        /// <summary>
        /// Allow a user to write data to the warehouse
        /// </summary>
        public const string QueryWarehouseData = UnrestrictedWarehouse + ".3";

        /// <summary>
        /// Write all materials
        /// </summary>
        public const string WriteMaterials = UnrestrictedMetadata + ".1.0";

        /// <summary>
        /// delete alll materials
        /// </summary>
        public const string DeleteMaterials = UnrestrictedMetadata + ".1.1";

        /// <summary>
        /// Read materials
        /// </summary>
        public const string ReadMaterials = ReadMetadata + ".1.2";

        /// <summary>
        /// Query materials
        /// </summary>
        public const string QueryMaterials = ReadMetadata + ".1.3";

        /// <summary>
        /// Write all facilities
        /// </summary>
        public const string WritePlacesAndOrgs = UnrestrictedMetadata + ".2.0";

        /// <summary>
        /// delete alll facilities
        /// </summary>
        public const string DeletePlacesAndOrgs = UnrestrictedMetadata + ".2.1";

        /// <summary>
        /// Read facilities
        /// </summary>
        public const string ReadPlacesAndOrgs = ReadMetadata + ".2.2";

        /// <summary>
        /// Query facilities
        /// </summary>
        public const string QueryPlacesAndOrgs = ReadMetadata + ".2.3";

        /// <summary>
        /// Export data from the CDR
        /// </summary>
        public const string ExportData = UnrestrictedAll + ".500";

        /// <summary>
        /// Override policy permission
        /// </summary>
        public const string OverridePolicyPermission = UnrestrictedAll + ".999";

        /// <summary>
        /// Security elevations serve as a special block whereby a user must re-enter their password to perform something
        /// </summary>
        public const string SecurityElevations = UnrestrictedAll + ".600";

        /// <summary>
        /// Policy identifier for allowing for the editing of an identity's security
        /// </summary>
        public const string AlterSecurityChallenge = SecurityElevations + ".1";

        #endregion IMS Policies in the 1.3.6.1.4.1.33349.3.1.5.9.2 namespace

        #region SanteDB Client Functions

        /// <summary>
        /// Access administrative function on the SanteDB Client
        /// </summary>
        public const string AccessClientAdministrativeFunction = UnrestrictedAll + ".10";

        /// <summary>
        /// Login to any facility 
        /// </summary>
        public const string LoginAnywhere = UnrestrictedAll + ".900";

        #endregion SanteDB Client Functions

        #region Stock Policies

        /// <summary>
        /// Unrestricted editing of administrative (non PHI) actions
        /// </summary>
        public const string UnrestrictedAdministrativeActs = UnrestrictedClinicalData + ".6";

        /// <summary>
        /// Write non PHI acts
        /// </summary>
        public const string WriteAdministrativeActs = UnrestrictedAdministrativeActs + ".1";

        /// <summary>
        /// Read non PHI acts
        /// </summary>
        public const string ReadAdministrativeActs = UnrestrictedAdministrativeActs + ".2";

        #endregion
    }
}