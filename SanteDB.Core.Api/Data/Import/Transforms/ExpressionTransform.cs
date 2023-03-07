using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DynamicExpresso;

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
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {

            // create an interpreter and execute
            var interpreter = new Interpreter(InterpreterOptions.Default)
                        .Reference(typeof(Guid))
                        .Reference(typeof(TimeSpan))
                        .EnableReflection();
            var arguments = Enumerable.Range(0, sourceRecord.ColumnCount).Select(o => new Parameter(sourceRecord.GetName(o), sourceRecord[o] ?? String.Empty)).ToArray();
            return interpreter.Parse(args[0].ToString(), arguments).Invoke(arguments.Select(o=>o.Value).ToArray());
        }
    }
}
