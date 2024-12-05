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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Privacy;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents the default password validation service feature
    /// </summary>
    public class SecurityServicesFeature : IFeature
    {

        // Add policies
        internal readonly Dictionary<SecurityPolicyIdentification, Func<Object>> m_securityPolicyConfigurations = new Dictionary<SecurityPolicyIdentification, Func<Object>>() {
            { SecurityPolicyIdentification.MaxPasswordAge, () => ConfigurationOptionType.Numeric },
              {   SecurityPolicyIdentification.PasswordHistory, () => ConfigurationOptionType.Boolean },
               {  SecurityPolicyIdentification.MaxInvalidLogins, () => ConfigurationOptionType.Numeric },
            { SecurityPolicyIdentification.AbandonSessionAfterPasswordReset, () => ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.AllowCachingOfUserCredentials, () => ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.AllowLocalDownstreamUserAccounts, () => ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.DefaultMfaMethod, () => ConfigurationOptionType.String },
            { SecurityPolicyIdentification.AllowNonAssignedUsersToLogin, () => ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.AllowNonAssignedUsersToLogin, () => ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.AllowPublicBackups, () => ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.AuditRetentionTime, () => Enumerable.Range(15, 365).Where(o => o % 15 == 0).Select(o => new PolicyValueTimeSpan(new TimeSpan(o, 0, 0, 0))) },
            { SecurityPolicyIdentification.DownstreamLocalSessionLength, () => Enumerable.Range(15, 180).Where(o => o % 15 == 0).Select(o => new PolicyValueTimeSpan(0, o, 0)) },
            { SecurityPolicyIdentification.RefreshLength, ()=>Enumerable.Range(15, 180).Where(o => o % 15 == 0).Select(o => new PolicyValueTimeSpan(0, o, 0)) },
            {  SecurityPolicyIdentification.RequireMfa, ()=>ConfigurationOptionType.Boolean },
            { SecurityPolicyIdentification.SessionLength,  ()=> Enumerable.Range(15, 180).Where(o => o % 15 == 0).Select(o => new PolicyValueTimeSpan(0, o, 0))}
        };

        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "Server Security";

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "Configures the default SanteDB server security features";

        /// <summary>
        /// Gets the group
        /// </summary>
        public string Group => FeatureGroup.Security;

        /// <summary>
        /// Configuration type
        /// </summary>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Gets or sets the flags
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.SystemFeature;

        /// <summary>
        /// Create the installation task
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[]
            {
                new InstallSecurityServicesTask(this),
                new InstallCertificatesTask(this)
            };
        }

        /// <summary>
        /// Create uninstallation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Query the current status of the feature
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {

            // Configuration of features
            var config = new GenericFeatureConfiguration();
            config.Options.Add("Configuration", () => ConfigurationOptionType.Object);
            var configSection = configuration.GetSection<SanteDB.Core.Security.Configuration.SecurityConfigurationSection>() ?? new SanteDB.Core.Security.Configuration.SecurityConfigurationSection()
            {
                Signatures = new List<SanteDB.Core.Security.Configuration.SecuritySignatureConfiguration>()
                {
                    new SanteDB.Core.Security.Configuration.SecuritySignatureConfiguration()
                    {
                        KeyName ="jwsdefault",
                        Algorithm = SanteDB.Core.Security.Configuration.SignatureAlgorithm.HS256,
                        HmacSecret = $"@SanteDBDefault$$${DateTime.Now.Year}_{Environment.MachineName}_{Guid.NewGuid()}"
                    }
                }
            };
            config.Values.Add("Configuration", configSection);

            // Add options for password encrypting and such
            config.Options.Add("PasswordHasher", () => AppDomain.CurrentDomain.GetAllTypes().Where(t => !t.IsInterface && !t.IsAbstract && typeof(IPasswordHashingService).IsAssignableFrom(t)));
            config.Options.Add("PolicyDecisionProvider", () => AppDomain.CurrentDomain.GetAllTypes().Where(t => !t.IsInterface && !t.IsAbstract && typeof(IPolicyDecisionService).IsAssignableFrom(t)));
            config.Options.Add("PolicyInformationProvider", () => AppDomain.CurrentDomain.GetAllTypes().Where(t => !t.IsInterface && !t.IsAbstract && typeof(IPolicyInformationService).IsAssignableFrom(t)));
            config.Options.Add("PasswordValidator", () => AppDomain.CurrentDomain.GetAllTypes().Where(t => !t.IsInterface && !t.IsAbstract && typeof(IPasswordValidatorService).IsAssignableFrom(t)));

            var appServices = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;

            Type validator = appServices.FirstOrDefault(t => typeof(IPasswordValidatorService).IsAssignableFrom(t.Type))?.Type,
                hasher = appServices.FirstOrDefault(t => typeof(IPasswordHashingService).IsAssignableFrom(t.Type))?.Type,
                pip = appServices.FirstOrDefault(t => typeof(IPolicyInformationService).IsAssignableFrom(t.Type))?.Type,
                pdp = appServices.FirstOrDefault(t => typeof(IPolicyDecisionService).IsAssignableFrom(t.Type))?.Type;

            config.Values.Add("PasswordHasher", hasher ?? typeof(SanteDB.Core.Security.SHA256PasswordHashingService));
            config.Values.Add("PasswordValidator", validator ?? typeof(RegexPasswordValidator));
            config.Values.Add("PolicyDecisionProvider", pdp ?? typeof(DefaultPolicyDecisionService));
            config.Values.Add("PolicyInformationProvider", pip);

            if (this.Configuration == null)
            {
                this.Configuration = config;
            }


            config.Categories.Add("Policies", m_securityPolicyConfigurations.Keys.Select(o => o.ToString()).ToArray());
            this.m_securityPolicyConfigurations.ForEach(o =>
            {
                config.Options.Add(o.Key.ToString(), o.Value);
                var opt = o.Value();
                object defaultValue = null;
                if(opt is ConfigurationOptionType cot)
                {
                    switch (cot)
                    {
                        case ConfigurationOptionType.Boolean:
                            defaultValue = false;
                            break;
                        case ConfigurationOptionType.Numeric:
                            defaultValue = 0;
                            break;
                        case ConfigurationOptionType.String:
                        case ConfigurationOptionType.Password:
                            defaultValue = String.Empty;
                            break;
                    }
                }
                else if(opt is IEnumerable enumer)
                {
                    defaultValue = enumer.OfType<Object>().FirstOrDefault();
                }
                config.Values.Add(o.Key.ToString(), configSection.GetSecurityPolicy(o.Key, defaultValue));
            });
            return hasher != null && validator != null && pdp != null && pip != null ? FeatureInstallState.Installed : FeatureInstallState.PartiallyInstalled;

        }

    /// <summary>
    /// Install security certificates
    /// </summary>
    private class InstallCertificatesTask : IConfigurationTask
    {

        /// <summary>
        /// Create a new install certificates features
        /// </summary>
        public InstallCertificatesTask(IFeature feature)
        {
            this.Feature = feature;
        }

        /// <summary>
        /// Description of the feature
        /// </summary>
        public string Description => "Install SanteDB's applet signing certificates into the machine's certificate store";

        /// <summary>
        /// The feature to be installed
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the nameof the feature
        /// </summary>
        public string Name => "Install Certificates";

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {

            SecurityExtensions.InstallCertsForChain();
            this.ProgressChanged?.Invoke(this, new SanteDB.Core.Services.ProgressChangedEventArgs(nameof(InstallCertificatesTask), 1.0f, "Complete"));

            return true;
        }

        /// <summary>
        /// Rollback
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <summary>
        /// Verify state
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }

    /// <summary>
    /// Install security services task
    /// </summary>
    private class InstallSecurityServicesTask : IConfigurationTask
    {

        // THe feature reference
        private SecurityServicesFeature m_feature;

        /// <summary>
        /// Creates a new installation task
        /// </summary>
        public InstallSecurityServicesTask(SecurityServicesFeature feature)
        {
            this.m_feature = feature;
        }

        /// <summary>
        /// Get the name of the service
        /// </summary>
        public string Name => "Save Security Configuration";

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "Installs and configures the core security services";

        /// <summary>
        /// Gets the feature
        /// </summary>
        public IFeature Feature => this.m_feature;

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var appServices = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            appServices.RemoveAll(t => typeof(IPasswordValidatorService).IsAssignableFrom(t.Type) ||
                    typeof(IPasswordHashingService).IsAssignableFrom(t.Type) ||
                    typeof(IPolicyDecisionService).IsAssignableFrom(t.Type));

            // Now we want to read the configuration 
            var config = this.m_feature.Configuration as GenericFeatureConfiguration;
            if (config == null)
            {
                this.m_feature.QueryState(configuration);
                config = this.m_feature.Configuration as GenericFeatureConfiguration;
            }

            configuration.RemoveSection<SanteDB.Core.Security.Configuration.SecurityConfigurationSection>();
            var secSection = config.Values["Configuration"] as SanteDB.Core.Security.Configuration.SecurityConfigurationSection;
            configuration.AddSection(secSection);

            // Now add the services
            appServices.RemoveAll(t => t.Type == typeof(ExemptablePolicyFilterService));
            appServices.Add(new TypeReferenceConfiguration(typeof(ExemptablePolicyFilterService)));
            appServices.Add(new TypeReferenceConfiguration(config.Values["PasswordHasher"] as Type));
            appServices.Add(new TypeReferenceConfiguration(config.Values["PasswordValidator"] as Type));
            appServices.Add(new TypeReferenceConfiguration(config.Values["PolicyDecisionProvider"] as Type));

            var pipService = config.Values["PolicyInformationProvider"] as Type;
            if (pipService == null)
            {
                pipService = appServices.Find(o => typeof(IPolicyInformationService).IsAssignableFrom(o.Type))?.Type;
                if (pipService != null)
                {
                    config.Values["PolicyInformationProvider"] = pipService;
                }
                else
                {
                    Tracer.GetTracer(this.GetType()).TraceWarning("You should assign a PIP implementation!");
                }
            }
            else
            {
                appServices.RemoveAll(o => typeof(IPolicyInformationService).IsAssignableFrom(o.Type));
                appServices.Add(new TypeReferenceConfiguration(pipService));
            }

                // Now set the policy values


                m_feature.m_securityPolicyConfigurations.ForEach(o =>
                {
                    secSection.SetPolicy(o.Key, config.Values[o.Key.ToString()]);
                });
            this.ProgressChanged?.Invoke(this, new SanteDB.Core.Services.ProgressChangedEventArgs(nameof(InstallSecurityServicesTask), 1.0f, "Complete"));

            return true;
        }

        /// <summary>
        /// Rollback configuration
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <summary>
        /// Verify state
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            return true;
        }
    }
}
}
