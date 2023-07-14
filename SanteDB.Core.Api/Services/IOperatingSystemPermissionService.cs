using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Permission types
    /// </summary>
    public enum OperatingSystemPermissionType
    {
        /// <summary>
        /// The application is demanding permission to access geo-location services
        /// </summary>
        GeoLocation,
        /// <summary>
        /// The application is demanding permission to access the file system
        /// </summary>
        FileSystem,
        /// <summary>
        /// The application is demanding permission to access the camera
        /// </summary>
        Camera,
        /// <summary>
        /// Bluetooth permissions
        /// </summary>
        Bluetooth
    }

    /// <summary>
    /// Represents a security service for the operating system
    /// </summary>
    public interface IOperatingSystemPermissionService
    {

        /// <summary>
        /// True if the current execution context has the requested permission
        /// </summary>
        bool HasPermission(OperatingSystemPermissionType permission);

        /// <summary>
        /// Request permission
        /// </summary>
        bool RequestPermission(OperatingSystemPermissionType permission);
    }
}
