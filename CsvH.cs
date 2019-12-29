using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Data.OleDb;
using System.Globalization;
using System.Diagnostics;

namespace VulmgmtCreateReport
{
    class CsvH
    {
        public DataTable GetDataTableFromCsv(string csvFile)
        {
            string pathOnly = Path.GetDirectoryName(csvFile);
            string fileName = Path.GetFileName(csvFile);

            DataTable dataTable = new DataTable();
            try
            {
                TextFieldParser reader = new TextFieldParser(csvFile);
                reader.SetDelimiters(new string[] { ";" });
                reader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = reader.ReadFields();
                foreach (string column in colFields)
                {
                    DataColumn datecolumn = new DataColumn(column);
                    datecolumn.AllowDBNull = true;
                    dataTable.Columns.Add(datecolumn);
                }
                while (!reader.EndOfData)
                {
                    string[] fieldData = reader.ReadFields();
                    dataTable.Rows.Add(fieldData);
                }
            }
            catch
            {
                return null;
            }
            return dataTable;
        }

    }
}
