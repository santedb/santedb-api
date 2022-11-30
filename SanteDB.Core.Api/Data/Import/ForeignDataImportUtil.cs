using SanteDB;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Foreign data format utility
    /// </summary>
    public class ForeignDataImportUtil
    {

        private static ForeignDataImportUtil s_current;
        private static object s_lockObject = new object();
        private readonly IDictionary<string, IForeignDataFormat> m_formats;
        private readonly Dictionary<string, IForeignDataElementTransform> m_transforms;

        /// <summary>
        /// Foreign data format utility ctor
        /// </summary>
        private ForeignDataImportUtil()
        {
            var serviceManager = ApplicationServiceContext.Current.GetService<IServiceManager>();
            if(serviceManager == null)
            {
                throw new InvalidOperationException("Missing service manager");
            }
            this.m_formats = serviceManager.CreateInjectedOfAll<IForeignDataFormat>().ToDictionary(o => o.FileExtension, o => o);
            this.m_transforms = serviceManager.CreateInjectedOfAll<IForeignDataElementTransform>().ToDictionary(o => o.Name, o => o);
        }


        /// <summary>
        /// Try to get the foreign data format handler from mime type
        /// </summary>
        /// <param name="fileExtension">The file extension</param>
        /// <param name="foreignDataFormat">The foreign data format</param>
        /// <returns>The foreign data format</returns>
        public bool TryGetDataFormat(string fileExtension, out IForeignDataFormat foreignDataFormat) => m_formats.TryGetValue(fileExtension, out foreignDataFormat);

        /// <summary>
        /// Try to get the element transformer
        /// </summary>
        /// <param name="transformName">The name of the formatter to get</param>
        /// <param name="foreignDataElementTransform">The foreign data element transformer</param>
        /// <returns>True if the transformer <paramref name="transformName"/> exists</returns>
        public bool TryGetElementTransformer(string transformName, out IForeignDataElementTransform foreignDataElementTransform) => this.m_transforms.TryGetValue(transformName, out foreignDataElementTransform);

        /// <summary>
        /// Get the current singleton instance
        /// </summary>
        public static ForeignDataImportUtil Current
        {
            get
            {
                if (s_current == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_current == null)
                        {
                            s_current = new ForeignDataImportUtil();
                        }
                    }
                }
                return s_current;
            }
        }
    }
}
