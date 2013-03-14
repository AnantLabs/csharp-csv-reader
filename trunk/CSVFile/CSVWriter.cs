﻿/*
 * 2006 - 2012 Ted Spence, http://tedspence.com
 * License: http://www.apache.org/licenses/LICENSE-2.0 
 * Home page: https://code.google.com/p/csharp-csv-reader/
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Reflection;

namespace CSVFile
{
    public class CSVWriter : IDisposable
    {
        protected char _delimiter, _text_qualifier;

        protected StreamWriter _outstream;

        #region Constructors
        /// <summary>
        /// Construct a new CSV writer to produce output on the enclosed StreamWriter
        /// </summary>
        public CSVWriter(StreamWriter source, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            _outstream = source;
            _delimiter = delim;
            _text_qualifier = qual;
        }

        /// <summary>
        /// Construct a new CSV reader to produce output on the specified stream
        /// </summary>
        public CSVWriter(Stream source, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            _outstream = new StreamWriter(source);
            _delimiter = delim;
            _text_qualifier = qual;
        }

        /// <summary>
        /// Initialize a new CSV file structure to write data to disk
        /// </summary>
        public CSVWriter(string filename, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            _outstream = new StreamWriter(filename);
            _delimiter = delim;
            _text_qualifier = qual;
        }
        #endregion

        #region Writing values
        /// <summary>
        /// Write one line to the file
        /// </summary>
        /// <param name="line">The array of values for this line</param>
        /// <param name="force_qualifiers">True if you want to force qualifiers for this line.</param>
        public void WriteLine(IEnumerable<object> line, bool force_qualifiers = false)
        {
            _outstream.WriteLine(CSV.Output(line, _delimiter, _text_qualifier, force_qualifiers));
        }

        /// <summary>
        /// Write the data table to a stream in CSV format
        /// </summary>
        /// <param name="dt">The data table to write</param>
        /// <param name="sw">The stream where the CSV text will be written</param>
        /// <param name="save_column_names">True if you wish the first line of the file to have column names</param>
        /// <param name="delim">The delimiter (comma, tab, pipe, etc) to separate fields</param>
        /// <param name="qual">The text qualifier (double-quote) that encapsulates fields that include delimiters</param>
        public void Write(DataTable dt, bool save_column_names, bool force_qualifiers = false)
        {
            // Write headers, if the caller requested we do so
            if (save_column_names) {
                List<string> headers = new List<string>();
                foreach (DataColumn col in dt.Columns) {
                    headers.Add(col.ColumnName);
                }
                WriteLine(headers, force_qualifiers);
            }

            // Now produce the rows
            foreach (DataRow dr in dt.Rows) {
                WriteLine(dr.ItemArray, force_qualifiers);
            }

            // Flush the stream
            _outstream.Flush();
        }

        /// <summary>
        /// Serialize a list of objects to CSV using this writer
        /// </summary>
        /// <typeparam name="IEnumerable">An IEnumerable that produces the list of objects to serialize.</typeparam>
        public void WriteObjects<T>(IEnumerable<T> list, bool save_column_names, bool force_qualifiers = false)
        {
            // Extract information about the type we're writing to disk
            Type list_type = typeof(T);
            FieldInfo[] filist = list_type.GetFields();
            PropertyInfo[] pilist = list_type.GetProperties();

            // Produce headers
            if (save_column_names) {
                List<string> headers = new List<string>();
                foreach (FieldInfo fi in filist) {
                    headers.Add(fi.Name);
                }
                foreach (PropertyInfo pi in pilist) {
                    headers.Add(pi.Name);
                }
                WriteLine(headers, force_qualifiers);
            }

            // Iterate through all the objects
            List<string> values = new List<string>();
            object val = null;
            foreach (T obj in list) {

                // Retrieve all the fields and properties
                values.Clear();
                foreach (FieldInfo fi in filist) {
                    val = fi.GetValue(obj);
                    if (val == null) {
                        values.Add("");
                    } else {
                        values.Add(val.ToString());
                    }
                }
                foreach (PropertyInfo pi in pilist) {
                    val = pi.GetValue(obj, null);
                    if (val == null) {
                        values.Add("");
                    } else {
                        values.Add(val.ToString());
                    }
                }

                // Output one line of CSV
                WriteLine(values, force_qualifiers);
            }

            // Flush the stream
            _outstream.Flush();
        }
        #endregion

        #region Disposables
        /// <summary>
        /// Close our resources - specifically, the stream reader
        /// </summary>
        public void Dispose()
        {
            _outstream.Flush();
            _outstream.Close();
            _outstream.Dispose();
        }
        #endregion
    }
}
