using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IBarcodeProviderService"/>
    /// </summary>
    public class DefaultBarcodeProviderService : IBarcodeProviderService
    {
        // Barcode generators
        private readonly IDictionary<String, IBarcodeGenerator> m_barcodeProviders;

        /// <inheritdoc/>
        public string ServiceName => "Barcode Provider Service";

        /// <summary>
        /// New instance of the barcode provider
        /// </summary>
        public DefaultBarcodeProviderService(IServiceManager serviceManager)
        {
            var barcodeProviders = serviceManager.CreateInjectedOfAll<IBarcodeGenerator>();
            this.m_barcodeProviders = barcodeProviders.ToDictionaryIgnoringDuplicates(o => o.BarcodeAlgorithm, o => o);
        }

        /// <inheritdoc/>
        public IBarcodeGenerator GetBarcodeGenerator(string barcodeAlgorithm)
        {
            this.m_barcodeProviders.TryGetValue(barcodeAlgorithm, out var retVal);
            return retVal;
        }
    }
}
