/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */
namespace SanteDB.Core
{
    /// <summary>
    /// SanteDB Host Type
    /// </summary>
    public enum SanteDBHostType
    {
        /// <summary>
        /// Environment is a server
        /// </summary>
        Server = 1,
        /// <summary>
        /// Environment is using client side API
        /// </summary>
        Client = 2,
        /// <summary>
        /// Environment is not in either client or server
        /// </summary>
        Other = 3,
        /// <summary>
        /// Environment is a test environment (like unit test)
        /// </summary>
        Test = 4,
        /// <summary>
        /// Environment is a gateway environment (allow external connections)
        /// </summary>
        Gateway = 5
    }
}