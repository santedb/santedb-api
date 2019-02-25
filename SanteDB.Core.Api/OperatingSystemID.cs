using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core
{
    /// <summary>
    /// Operating systems
    /// </summary>
    public enum OperatingSystemID
    {
        /// <summary>
        /// Host is running on Win32
        /// </summary>
        Win32 = 0x1,
        /// <summary>
        /// Host is running on Linux
        /// </summary>
        Linux = 0x2,
        /// <summary>
        /// Host is running on MacOS
        /// </summary>
        MacOS = 0x4,
        /// <summary>
        /// Host is running on Android
        /// </summary>
        Android = 0x8
    }

}
