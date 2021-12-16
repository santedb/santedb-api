using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A configuration feature which configures the data privacy controls
    /// </summary>
    public class DataPrivacyFilterFeature : GenericServiceFeature<SanteDB.Core.Security.Privacy.DataPolicyFilterService>
    {
        /// <summary>
        /// Gets the type of configuration
        /// </summary>
        public override Type ConfigurationType => typeof(SanteDB.Core.Security.Configuration.DataPolicyFilterConfigurationSection);

        /// <inerhitdoc/>
        public override string Group => FeatureGroup.Security;

        /// <summary>
        /// Get the default configuration for the object
        /// </summary>
        protected override object GetDefaultConfiguration()
        {
            return new SanteDB.Core.Security.Configuration.DataPolicyFilterConfigurationSection()
            {
                DefaultAction = Security.Configuration.ResourceDataPolicyActionType.Hide,
                Resources = new List<Security.Configuration.ResourceDataPolicyFilter>()
                {
                    new Security.Configuration.ResourceDataPolicyFilter()
                    {
                        ResourceType = new ResourceTypeReferenceConfiguration(typeof(AssigningAuthority)),
                        Action = Security.Configuration.ResourceDataPolicyActionType.Redact
                    }
                }
            };
        }
    }
}
