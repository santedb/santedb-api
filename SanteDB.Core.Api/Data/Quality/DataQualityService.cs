/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Jobs;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// The data quality service handler
    /// </summary>
    public class DataQualityService : IDaemonService
    {

        // Configuration
        private DataQualityConfigurationSection m_configuration;

        // Data quality configuration provider
        private IDataQualityConfigurationProviderService m_dataQualityConfigurationProvider;

        // Service manager
        private IServiceManager m_serviceManager;

        /// <summary>
        /// Create new data quality service
        /// </summary>
        public DataQualityService(IConfigurationManager configurationManager, IServiceManager serviceProvider, IDataQualityConfigurationProviderService configurationProvider = null)
        {
            this.m_configuration = configurationManager.GetSection<DataQualityConfigurationSection>();
            if (configurationProvider == null)
            {
                configurationProvider = serviceProvider.CreateInjected<LegacyRulesetConfigurationProvider>();
                serviceProvider.AddServiceProvider(configurationProvider);
            }

            this.m_dataQualityConfigurationProvider = configurationProvider;
            this.m_serviceManager = serviceProvider;
        }

        // Data quality service
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityService));

        /// <summary>
        /// Ruleset evaluators
        /// </summary>
        private List<IBusinessRulesService> m_attachedRules = new List<IBusinessRulesService>();

        /// <summary>
        /// True if the service is running
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName => "Data Quality Service";

        /// <summary>
        /// Daemon is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Daemon has started
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Daemon is stopping
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Daemon has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Start the daemon service
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            if (this.m_configuration != null)
            {
                ApplicationServiceContext.Current.AddBusinessRule(typeof(DataQualityBundleRule));
                // Iterate over rule sets and regiser their rules as BRE
                foreach (var ruleType in this.m_dataQualityConfigurationProvider.GetRuleSets().Where(t => t.Enabled).SelectMany(o => o.Resources).Select(o => o.ResourceType).Distinct())
                {
                    this.m_tracer.TraceInfo("Initializing ruleset {0}", ruleType.Name);
                    // Iterate over resources in rule set and register BRE if not already done

                    var breType = typeof(DataQualityBusinessRule<>).MakeGenericType(ruleType);
                    var current = ApplicationServiceContext.Current.GetService(breType) as IBusinessRulesService;

                    // If the current is null we want to add the service
                    if (current == null)
                    {
                        current = ApplicationServiceContext.Current.AddBusinessRule(breType) as IBusinessRulesService;
                        this.m_attachedRules.Add(current);
                    }
                }
            }

            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                ApplicationServiceContext.Current.GetService<IJobManagerService>().AddJob(new DataQualityExtensionCleanJob(), new TimeSpan(1, 0, 0));
            };
            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stop the daemon service
        /// </summary>
        public bool Stop()
        {
            this.Started?.Invoke(this, EventArgs.Empty);

            foreach (var itm in this.m_attachedRules)
                ApplicationServiceContext.Current.GetService<IServiceManager>().RemoveServiceProvider(itm.GetType());

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
