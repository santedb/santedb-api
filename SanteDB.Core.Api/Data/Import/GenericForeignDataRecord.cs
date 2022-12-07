using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A generic implementation of the <see cref="IForeignDataRecord"/>
    /// </summary>
    public class GenericForeignDataRecord : IForeignDataRecord
    {

        private readonly string[] m_columnNames;
        private object[] m_values;

        /// <summary>
        /// Create a new generic data record
        /// </summary>
        public GenericForeignDataRecord(String[] columnNames)
        {
            this.m_columnNames = columnNames;
            this.m_values = new object[this.m_columnNames.Length];
        }

        /// <summary>
        /// Create a generic data record from another reader
        /// </summary>
        /// <param name="copyFrom">The reader from which columns and data should be copied</param>
        /// <param name="extraColumns">The additional columns to add to the record set</param>
        public GenericForeignDataRecord(IForeignDataReader copyFrom, params string[] extraColumns)
        {
            this.m_columnNames = Enumerable.Range(0, copyFrom.ColumnCount).Select(o => copyFrom.GetName(o)).Union(extraColumns).ToArray();
            this.m_values = Enumerable.Range(0, copyFrom.ColumnCount).Select(o => copyFrom[o]).Concat(new object[extraColumns.Length]).ToArray();
        }

        /// <inheritdoc/>
        public object this[string name]
        {
            get
            {
                var colIndex = this.IndexOf(name);
                if (colIndex == -1)
                {
                    return null;
                }
                return this.m_values[colIndex];
            }
            set
            {
                var colIndex = this.IndexOf(name);
                if (colIndex == -1)
                {
                    throw new MissingFieldException(name);
                }
                this.m_values[colIndex] = value;
            }
        }

        /// <inheritdoc/>
        public object this[int index]
        {
            get => this.m_values[index];
            set => this.m_values[index] = value;
        }

        /// <inheritdoc/>
        public int ColumnCount => this.m_columnNames.Length;

        /// <inheritdoc/>
        public string GetName(int index) => this.m_columnNames[index];

        /// <inheritdoc/>
        public int IndexOf(string name) => Array.IndexOf(this.m_columnNames, name);
    }
}
