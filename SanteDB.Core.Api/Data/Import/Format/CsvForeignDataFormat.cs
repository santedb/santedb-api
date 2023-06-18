/*
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
using SanteDB;
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SanteDB.Core.Data.Import.Format
{
    /// <summary>
    /// Foreign data format that is for comma-separated-values
    /// </summary>
    public class CsvForeignDataFormat : IForeignDataFormat
    {


        /// <summary>
        /// Foreign data reader for CSV files
        /// </summary>
        private class CsvForiegnDataReader : IForeignDataReader
        {

            private static readonly Regex s_columnExtract = new Regex(@"((?:""(?:(?:[^""]|""""|\w)*)"")|[\d\.]+|true|false|[^,]+)?,", RegexOptions.Compiled);
            private int m_rowsRead = 0;
            private StreamReader m_source;
            private object m_syncLock = new object();
            private bool m_isDisposed = false;
            private string[] m_columnNames;
            private object[] m_values;

            /// <inheritdoc/>
            public string SubsetName => String.Empty;

            /// <summary>
            /// Throw <see cref="ObjectDisposedException"/> if this is disposed
            /// </summary>
            private void ThrowIfDisposed()
            {
                if (m_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(CsvForeignDataWriter));
                }
            }

            /// <summary>
            /// Create a new foreign data reader for CSV from source
            /// </summary>
            /// <param name="source">The source stream</param>
            public CsvForiegnDataReader(Stream source)
            {
                m_source = new StreamReader(source);
            }

            /// <inheritdoc/>
            object IForeignDataRecord.this[string name]
            {
                get => this[name];
                set => throw new NotSupportedException();
            }

            /// <inheritdoc/>
            object IForeignDataRecord.this[int index]
            {
                get => this[index];
                set => throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public object this[string name]
            {
                get
                {
                    this.ThrowIfDisposed();
                    this.ReadSchema();
                    var colIndex = IndexOf(name);
                    if (colIndex == -1)
                    {
                        throw new MissingFieldException(name);
                    }
                    return m_values[colIndex];
                }
            }

            /// <inheritdoc/>
            public object this[int index]
            {
                get
                {
                    this.ThrowIfDisposed();
                    this.ReadSchema();
                    return m_values[index];
                }
            }

            /// <inheritdoc/>
            public int RowNumber
            {
                get
                {
                    this.ThrowIfDisposed();
                    this.ReadSchema();
                    return this.m_rowsRead;
                }
            }

            /// <inheritdoc/>
            public int ColumnCount
            {
                get
                {
                    this.ThrowIfDisposed();
                    this.ReadSchema();
                    return m_columnNames.Length;
                }
            }

            /// <inheritdoc/>
            public string GetName(int index)
            {
                this.ThrowIfDisposed();
                this.ReadSchema();
                return m_columnNames[index];
            }

            /// <inheritdoc/>
            public int IndexOf(string name)
            {
                this.ThrowIfDisposed();
                this.ReadSchema();
                return Array.IndexOf(m_columnNames, name);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                m_source.Dispose();
                m_isDisposed = true;
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                if (m_source.EndOfStream)
                {
                    return false;
                }

                lock (this.m_syncLock)
                {
                    if (m_columnNames == null)
                    {
                        this.ReadSchema();
                        return MoveNext();
                    }

                    this.m_rowsRead++;

                    this.m_values = s_columnExtract.Matches($"{this.m_source.ReadLine()},")
                        .OfType<Match>()
                        .Select(o => UnescapeValue(o.Groups[1].Value))
                        .ToArray();
                    return true;
                }
            }

            /// <summary>
            /// Read schema
            /// </summary>
            private void ReadSchema()
            {
                if (!this.m_source.EndOfStream && this.m_columnNames == null && this.m_rowsRead == 0)
                {
                    lock (this.m_syncLock)
                    {
                        m_columnNames = s_columnExtract
                                .Matches($"{this.m_source.ReadLine()},")
                                .OfType<Match>()
                                .Select(o => UnescapeValue(o.Groups[1].Value))
                                .OfType<string>()
                                .ToArray();
                    }
                }
            }

            /// <summary>
            /// Unescape a value
            /// </summary>
            private object UnescapeValue(string csvValue)
            {
                if (String.IsNullOrEmpty(csvValue))
                {
                    return null;
                }
                else if (csvValue.StartsWith("\""))
                {
                    return csvValue.Substring(1, csvValue.Length - 2).Replace("\"\"", "");
                }
                else if (bool.TryParse(csvValue, out var bl))
                {
                    return bl;
                }
                else if (long.TryParse(csvValue, out var lv))
                {
                    return lv;
                }
                else if (double.TryParse(csvValue, out var dbl))
                {
                    return dbl;
                }
                else if (TimeSpan.TryParse(csvValue, out var ts))
                {
                    return ts;
                }
                else if (DateTime.TryParse(csvValue, out var dt))
                {
                    return dt;
                }
                return csvValue;
            }
        }

        /// <summary>
        /// Foreign data writer for CSV files
        /// </summary>
        private class CsvForeignDataWriter : IForeignDataWriter
        {
            private readonly StreamWriter m_stream;
            private bool m_isDisposed = false;
            private int m_rowNumber = 0;
            private object m_syncLock = new object();
            private string[] m_columnNames;

            /// <summary>
            /// Throw <see cref="ObjectDisposedException"/> if this is disposed
            /// </summary>
            private void ThrowIfDisposed()
            {
                if (m_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(CsvForeignDataWriter));
                }
            }

            /// <summary>
            /// Comma-saved value writer
            /// </summary>
            /// <param name="sourceStream">The stream which contains the data</param>
            public CsvForeignDataWriter(Stream sourceStream)
            {
                m_stream = new StreamWriter(sourceStream, System.Text.Encoding.UTF8, 1024, true);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                m_stream.Dispose();
                m_isDisposed = true;
            }

            /// <inheritdoc/>
            public bool WriteRecord(IForeignDataRecord foreignDataRecord)
            {
                ThrowIfDisposed();

                lock (m_syncLock)
                {
                    if (m_rowNumber++ == 0) // emit header
                    {
                        this.m_columnNames = Enumerable.Range(0, foreignDataRecord.ColumnCount).Select(o => foreignDataRecord.GetName(o)).ToArray();
                        m_stream.WriteLine(
                            string.Join(",", this.m_columnNames.Select(o => FormatValue(o)))
                        );
                    }

                    m_stream.WriteLine(
                        string.Join(",", this.m_columnNames.Select(o => this.FormatValue(foreignDataRecord[o])))
                    );
                }
                return true;
            }

            /// <summary>
            /// Format the value for CSV writing
            /// </summary>
            private string FormatValue(object value)
            {
                if (value == null)
                {
                    return String.Empty;
                }

                switch (value)
                {
                    case DateTime dt:
                        return XmlConvert.ToString(dt, XmlDateTimeSerializationMode.Utc);
                    case DateTimeOffset dto:
                        return XmlConvert.ToString(dto);
                    case bool bl:
                        return XmlConvert.ToString(bl);
                    case string str:
                        return $"\"{str.Replace("\"", "\"\"")}\"";
                    default:
                        return value.ToString();
                }
            }

            /// <summary>
            /// Get the number of records written (excluding the header row)
            /// </summary>
            public int RecordsWritten => this.m_rowNumber;
        }

        /// <summary>
        /// Foreign data file for CSV files
        /// </summary>
        private class CsvForeignDataFile : IForeignDataFile
        {
            // The underlying stream
            private readonly Stream m_stream;
            private bool m_isDisposed = false;
            private IDisposable m_openReaderOrWriter = null;

            /// <summary>
            /// Create a new foreign data file for CSV
            /// </summary>
            /// <param name="source">The source stream</param>
            public CsvForeignDataFile(Stream source)
            {
                m_stream = source;
            }

            /// <summary>
            /// Throw an <see cref="ObjectDisposedException"/> if this data file is disposed
            /// </summary>
            /// <exception cref="ObjectDisposedException">The disposed file</exception>
            internal void ThrowIfDisposed()
            {
                if (m_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(CsvForeignDataFile));
                }
            }

            /// <inheritdoc/>
            public IForeignDataReader CreateReader(string subsetName = null)
            {
                ThrowIfDisposed();

                if (!string.IsNullOrEmpty(subsetName))
                {
                    throw new ArgumentOutOfRangeException(string.Format(ErrorMessages.ARGUMENT_OUT_OF_RANGE, subsetName, "String.Empty"));
                }
                else if (m_openReaderOrWriter != null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(CreateReader)));
                }
                else if (!m_stream.CanRead)
                {
                    throw new InvalidOperationException(ErrorMessages.CANT_READ_WRITE_ONLY_STREAM);
                }
                m_openReaderOrWriter = new CsvForiegnDataReader(m_stream);
                return (IForeignDataReader)m_openReaderOrWriter;
            }

            /// <inheritdoc/>
            public IForeignDataWriter CreateWriter(string subsetName = null)
            {
                ThrowIfDisposed();

                if (!string.IsNullOrEmpty(subsetName))
                {
                    throw new ArgumentOutOfRangeException(string.Format(ErrorMessages.ARGUMENT_OUT_OF_RANGE, subsetName, "String.Empty"));
                }
                else if (m_openReaderOrWriter != null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(CreateWriter)));
                }
                else if (!m_stream.CanWrite)
                {
                    throw new InvalidOperationException(ErrorMessages.CANT_WRITE_READ_ONLY_STREAM);
                }
                m_openReaderOrWriter = new CsvForeignDataWriter(m_stream);
                return (IForeignDataWriter)m_openReaderOrWriter;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                m_openReaderOrWriter?.Dispose();
                m_stream.Dispose();
                m_isDisposed = true;
            }

            /// <inheritdoc/>
            /// <remarks>CSV files don't support subsets (sheets, tables, etc.)</remarks>
            public IEnumerable<string> GetSubsetNames()
            {
                yield return String.Empty;
            }
        }

        /// <inheritdoc/>
        public string FileExtension => ".csv";

        /// <inheritdoc/>
        public IForeignDataFile Open(Stream foreignDataStream)
        {
            if (foreignDataStream == null)
            {
                throw new ArgumentNullException(nameof(foreignDataStream));
            }

            return new CsvForeignDataFile(foreignDataStream);
        }

    }
}
