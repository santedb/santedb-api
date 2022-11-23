using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import.Format
{
    /// <summary>
    /// Foreign data format utility
    /// </summary>
    public class ForeignDataFormatUtil
    {

        private static ForeignDataFormatUtil s_current;
        private static object s_lockObject = new object();
        private readonly IDictionary<string, IForeignDataFormat> m_formats;

        /// <summary>
        /// Foreign data format utility ctor
        /// </summary>
        private ForeignDataFormatUtil()
        {
            this.m_formats = AppDomain.CurrentDomain.GetAllTypes()
                .Where(t => t.Implements(typeof(IForeignDataFormat)) && !t.IsInterface && !t.IsAbstract)
                .Select(t => Activator.CreateInstance(t) as IForeignDataFormat)
                .ToDictionary(o => o.MimeType, o=>o);
        }


        /// <summary>
        /// Try to get the foreign data format handler from mime type
        /// </summary>
        /// <param name="mimeType">The mime type</param>
        /// <param name="foreignDataFormat">The foreign data format</param>
        /// <returns>The foreign data format</returns>
        public bool TryGetFromMimeType(String mimeType, out IForeignDataFormat foreignDataFormat)
        {
            return this.m_formats.TryGetValue(mimeType, out foreignDataFormat);
        }

        /// <summary>
        /// Get the current singleton instance
        /// </summary>
        public static ForeignDataFormatUtil Current
        {
            get
            {
                if(s_current == null)
                {
                    lock(s_lockObject)
                    {
                        if(s_current == null)
                        {
                            s_current = new ForeignDataFormatUtil();
                        }
                    }
                }
                return s_current;
            }
        }
    }
}
