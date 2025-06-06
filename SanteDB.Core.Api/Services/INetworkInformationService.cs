﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents network interface information
    /// </summary>
    public struct NetworkInterfaceInfo
    {

        /// <summary>
        /// Network interface ctor
        /// </summary>
        public NetworkInterfaceInfo(String name, String macAddress, bool isActive, String manufacturer, string ipAddress, string gateway, NetworkInterfaceType networkInterfaceType)
        {
            this.Name = name;
            this.MacAddress = macAddress;
            this.IsActive = isActive;
            this.Manufacturer = manufacturer;
            this.IpAddress = ipAddress;
            this.Gateway = gateway;
            this.InterfaceType = networkInterfaceType;
        }

        /// <summary>
        /// IpAddress
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Mac address
        /// </summary>
        public String MacAddress { get; private set; }

        /// <summary>
        /// Gets or sets the name of the interface
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Indicates whether the interface is connected
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Manufacturer
        /// </summary>
        public String Manufacturer { get; private set; }

        /// <summary>
        /// Gets or sets the gateway 
        /// </summary>
        public String Gateway { get; private set; }

        /// <summary>
        /// Gets the interface type
        /// </summary>
        public NetworkInterfaceType InterfaceType { get; }
    }

    /// <summary>
    /// Represents network information service 
    /// </summary>
    [System.ComponentModel.Description("Network Metadata Provider")]
    public interface INetworkInformationService : IServiceImplementation
    {

        /// <summary>
        /// Get interface information 
        /// </summary>
        IEnumerable<NetworkInterfaceInfo> GetInterfaces();

        /// <summary>
        /// Pings the specified host
        /// </summary>
        long Ping(String hostName);

        /// <summary>
        /// Gets whether the network is available
        /// </summary>
        bool IsNetworkAvailable { get; }

        /// <summary>
        /// Gets whether the network is connected.
        /// </summary>
        bool IsNetworkConnected { get; }

        /// <summary>
        /// Returns true if the network is WIFI
        /// </summary>
        bool IsNetworkWifi { get; }

        /// <summary>
        /// Fired when the network status changes
        /// </summary>
        event EventHandler NetworkStatusChanged;

        /// <summary>
        /// Perform a DNS lookup
        /// </summary>
        string Nslookup(string address);

        /// <summary>
        /// Get the hostname of the local host
        /// </summary>
        string GetHostName();

        /// <summary>
        /// Gets the machines name
        /// </summary>
        string GetMachineName();
    }
}
