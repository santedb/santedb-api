/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using DynamicExpresso;
using System;
using System.Linq;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// Expression transformation
    /// </summary>
    public class ExpressionTransform : IForeignDataElementTransform
    {
        /// <inheritdoc/>
        public string Name => "Expression";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, System.Collections.Generic.IDictionary<string, string> dataMapParameters, params object[] args)
        {

            // create an interpreter and execute
            var interpreter = new Interpreter(InterpreterOptions.Default)
                        .Reference(typeof(Guid))
                        .Reference(typeof(TimeSpan))
                        .EnableReflection();
            var arguments = new Parameter[]
            {
                new Parameter("input", input),
                new Parameter("parameters", dataMapParameters),
                new Parameter("source", sourceRecord)
            };
            return interpreter.Parse(args[0].ToString(), arguments).Invoke(arguments.Select(o => o.Value).ToArray());
        }
    }
}
