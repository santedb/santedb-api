/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Collections;
using System.Collections.Generic;

namespace SanteDB.Core.ViewModel
{
    /// <summary>
    /// Represents a classifier which is responsible for taking a particular instance of type X
    /// and determining the appropriate classification
    /// </summary>
    public interface IViewModelClassifier
    {

        /// <summary>
        /// Gets the type this classifier handles
        /// </summary>
        Type HandlesType { get; }

        /// <summary>
        /// Gets the appropriate classifier for the specified data
        /// </summary>
        Dictionary<string, IList> Classify(IList data);

        /// <summary>
        /// Re-compose the classified data 
        /// </summary>
        IList Compose(Dictionary<string, object> values, Type retValType);
    }
}