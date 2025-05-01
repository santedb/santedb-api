/*
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
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{

    /// <summary>
    /// Identifies the 
    /// </summary>
    public enum TfaMechanismClassification
    {
        /// <summary>
        /// Indicates the TFA mechanism is an application (such as an authenticator) that the user runs on a device
        /// </summary>
        Application,
        /// <summary>
        /// Indicates that the mechansim is conveyed via sending a message to the user 
        /// </summary>
        Message
    }
    /// <summary>
    /// Represents the TFA mechanism
    /// </summary>
    public interface ITfaMechanism
    {

        /// <summary>
        /// Gets the identifier of the mechanism
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the classification of this TFA mechanism
        /// </summary>
        TfaMechanismClassification Classification { get; }

        /// <summary>
        /// Get the host types that this mechanism is compatible with
        /// </summary>
        SanteDBHostType[] HostTypes { get; }

        /// <summary>
        /// Gets the name of the TFA mechanism
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Get the text which should be used to display as help
        /// </summary>
        String SetupHelpText { get; }

        /// <summary>
        /// Send the specified two factor authentication via the mechanism 
        /// </summary>
        /// <param name="user">The user to send the TFA secret for</param>
        /// <returns>Special instructional text</returns>
        String Send(IIdentity user);

        /// <summary>
        /// Validate the secret
        /// </summary>
        bool Validate(IIdentity user, string secret);

        /// <summary>
        /// Create a shared secret to share with the user - this will be rendered as a QR code
        /// </summary>
        /// <param name="user">The user which is to have the shared secret established</param>
        /// <returns>The contents of a QR code to validate</returns>
        String BeginSetup(IIdentity user);

        /// <summary>
        /// Complete the setup procedure by ensuring that the user was able to generate and share the configured code
        /// </summary>
        /// <param name="user">The user for which setup is being completed</param>
        /// <param name="verificationCode">The verification code sent to the user</param>
        /// <returns>True if setup was completed</returns>
        bool EndSetup(IIdentity user, String verificationCode);
    }
}
