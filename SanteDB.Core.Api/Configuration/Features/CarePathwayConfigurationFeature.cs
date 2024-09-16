using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a configuration feature which enables the <see cref="DefaultCarepathEnrolmentService"/>
    /// </summary>
    public class CarePathwayConfigurationFeature : GenericServiceFeature<DefaultCarepathEnrolmentService>
    {
        /// <inheritdoc/>
        public override Type ConfigurationType => typeof(CarePathwayConfigurationSection);

        /// <inheritdoc/>
        public override string Group => FeatureGroup.System;

        /// <inheritdoc/>
        protected override object GetDefaultConfiguration()
        {
            return new CarePathwayConfigurationSection()
            {
                EnableAutoEnrollment = true
            };
        }
    }
}
