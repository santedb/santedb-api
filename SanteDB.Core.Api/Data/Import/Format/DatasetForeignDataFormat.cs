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
using SanteDB.Core.Data.Initialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Data.Import.Format
{
    /// <summary>
    /// Foreign data format that is for dataset files
    /// </summary>
    public class DatasetForeignDataFormat : IForeignDataFormat
    {

        /// <summary>
        /// Foreign data reader for dataset files
        /// </summary>
        private class DatasetForeignDataWriter : IForeignDataWriter
        {
            /// <inheritdoc/>
            public int RecordsWritten => throw new NotSupportedException();

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <inheritdoc/>
            public bool WriteRecord(IForeignDataRecord foreignDataRecord)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Foreign data reader for dataset files
        /// </summary>
        private class DatasetForeignDataReader : IForeignDataReader, IForeignDataBulkReader
        {
            private Dataset m_dataset;
            private int m_rnumber = -1;
            private const string SELF_COL_NAME = "$";

            /// <summary>
            /// Load the source stream
            /// </summary>
            public DatasetForeignDataReader(Stream sourceStream)
            {
                this.m_dataset = Dataset.Load(sourceStream);
            }

            /// <inheritdoc/>
            public object this[string name] => name == SELF_COL_NAME ? this.m_dataset.Action[m_rnumber] : null;

            /// <inheritdoc/>
            public object this[int index] => index == 0 ? this.m_dataset.Action[m_rnumber] : null;

            /// <inheritdoc/>
            object IForeignDataRecord.this[string name] { get => this[name]; set => throw new NotSupportedException(); }
            /// <inheritdoc/>
            object IForeignDataRecord.this[int index] { get => this[index]; set => throw new NotSupportedException(); }

            /// <inheritdoc/>
            public string SubsetName => String.Empty;

            /// <inheritdoc/>
            public int RowNumber => this.m_rnumber;

            /// <inheritdoc/>
            public int ColumnCount => 1;

            /// <inheritdoc/>
            public void Dispose()
            {
                this.m_dataset = null;
            }

            /// <inheritdoc/>
            public string GetName(int index) => index == 0 ? SELF_COL_NAME : null;

            /// <inheritdoc/>
            public int IndexOf(string name) => name == SELF_COL_NAME ? 0 : -1;

            /// <inheritdoc/>
            public bool MoveNext()
            {
                return this.m_dataset.Action.Count > ++this.m_rnumber;
            }

            /// <summary>
            /// Read as a dataset
            /// </summary>
            /// <returns></returns>
            public Dataset ReadAsDataset() => this.m_dataset;

            public void AddComputedColumn(string columnName, Func<IForeignDataReader, object> computation)
            {
                throw new NotImplementedException();
            }

            public void ClearComputedColumns() { }

            public bool HasComputedColumn(string columnName) => false;
        }

        /// <summary>
        /// Foreign data file for dataset files
        /// </summary>
        private class DatasetForeignDataFile : IForeignDataFile
        {

            private Stream m_sourceStream;

            public DatasetForeignDataFile(Stream foreignDataStream)
            {
                this.m_sourceStream = foreignDataStream;
            }

            /// <inheritdoc/>
            public IForeignDataReader CreateReader(string subsetName = null)
            {
                return new DatasetForeignDataReader(this.m_sourceStream);
            }

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Writing to datasets not supported</exception>
            public IForeignDataWriter CreateWriter(string subsetName = null)
            {
                return new DatasetForeignDataWriter();
            }

            /// <inheritdoc/>
            public void Dispose()
            {

            }

            /// <inheritdoc/>
            public IEnumerable<string> GetSubsetNames()
            {
                yield return String.Empty;
            }
        }

        /// <inheritdoc/>
        public string FileExtension => ".dataset";

        /// <inheritdoc/>
        public IForeignDataFile Open(Stream foreignDataStream)
        {
            if (foreignDataStream == null)
            {
                throw new ArgumentNullException(nameof(foreignDataStream));
            }

            return new DatasetForeignDataFile(foreignDataStream);
        }

    }
}
