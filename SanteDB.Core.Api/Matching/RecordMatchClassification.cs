﻿/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
namespace SanteDB.Core.Matching
{
    /// <summary>
    /// Record match classifications
    /// </summary>
    public enum RecordMatchClassification
    {
        /// <summary>
        /// The records match
        /// </summary>
        Match = 0x2,
        /// <summary>
        /// There is a probable match
        /// </summary>
        Probable = 0x1,
        /// <summary>
        /// The is a non-match
        /// </summary>
        NonMatch = 0x0
    }
}
