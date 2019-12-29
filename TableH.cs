using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace NRSoft.FunctionPool
{
    public class TableH
    {
        static string configFile = "config.xml";
        static Config config = Config.Load(configFile);

        public TableH(Config config)
        {
           
        }

        public static DataTable GetOverrides()
        {
            string theDate = Program.GetDateFirstOfMonth();
            string csvPath = config.CsvPath;

            CsvH csvH = new CsvH();
            DataRow dataRow;

            string csvOverrides = Program.ValidatePath(csvPath) + theDate + "_overrides.csv";
            Console.WriteLine("processing overrides.csv...");
            if (!File.Exists(csvOverrides))
            {
                Console.WriteLine("could not find {0}", csvOverrides);
                Console.WriteLine("Exit progamm!");
                Console.ReadKey();
                return null;
            }
            DataTable tableOverrides = csvH.GetDataTableFromCsv(csvOverrides);
            Console.WriteLine("read {0} lines", tableOverrides.Rows.Count);
            Console.WriteLine("column count={0}", tableOverrides.Columns.Count);
            foreach (var col in tableOverrides.Columns)
            {
                Console.Write(col + "|");
            }
            Console.WriteLine();
            dataRow = tableOverrides.Rows[2];
            Console.WriteLine(dataRow[1]);

            return tableOverrides;
        }

        public static DataTable GetFindings()
        {
            string theDate = Program.GetDateFirstOfMonth();
            string csvPath = config.CsvPath;

            CsvH csvH = new CsvH();
            DataRow dataRow;

            string csvFindings = Program.ValidatePath(csvPath) + theDate + "_findings.csv";
            Console.WriteLine("processing findings.csv...");
            if (!File.Exists(csvFindings))
            {
                Console.WriteLine("could not find {0}", csvFindings);
                Console.WriteLine("Exit progamm!");
                Console.ReadKey();
                return null;
            }

            DataTable tableFindings = csvH.GetDataTableFromCsv(csvFindings);
            Console.WriteLine("read {0} lines", tableFindings.Rows.Count);
            Console.WriteLine("column count={0}", tableFindings.Columns.Count);
            foreach (var col in tableFindings.Columns)
            {
                Console.Write(col + "|");
            }
            Console.WriteLine();
            dataRow = tableFindings.Rows[2];
            Console.WriteLine(dataRow[1]);

            return tableFindings;

        }

        public static DataTable GetSubnetTable()
        {
            // GetSubnetFromOverride
            Console.WriteLine("processing table SubNetworks...");

            SqlH sqlH = new SqlH(config);

            // SqlConnection conn = sqlH.NewConnection();
            string connection = sqlH.GetBuildConnectionString();
            SqlConnection conn = new SqlConnection(connection);

            if (conn == null)
            {
                return null;
            }

            Debug.Print(conn.ConnectionString);
            Debug.Print(conn.State.ToString());

            string command = "SELECT DISTINCT IPSubnet, '' AS IPFirst, '' AS IPLast FROM interim.override WHERE  (IPSubnet > '')";

            SqlCommand sqlcmd = new SqlCommand();
            sqlcmd.Connection = conn;
            sqlcmd.CommandType = CommandType.Text;
            sqlcmd.CommandText = command;
            conn.Open();

            SqlDataReader dr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection);
            DataTable dtSchema = dr.GetSchemaTable();
            DataTable dt = new DataTable();
            List<DataColumn> listCols = new List<DataColumn>();

            if (dtSchema != null)
            {
                foreach (DataRow row in dtSchema.Rows)
                {
                    string columnName = System.Convert.ToString(row["ColumnName"]);
                    DataColumn column = new DataColumn(columnName, (Type)(row["DataType"]));
                    column.Unique = (bool)row["IsUnique"];
                    column.AllowDBNull = (bool)row["AllowDBNull"];
                    column.AutoIncrement = (bool)row["IsAutoIncrement"];
                    listCols.Add(column);
                    dt.Columns.Add(column);
                }
            }

            while (dr.Read())
            {
                DataRow row = dt.NewRow();
                for (int i = 0; i < listCols.Count; i++)
                {
                    row[((DataColumn)listCols[i])] = dr[i];
                }
                dt.Rows.Add(row);
            }

            foreach (DataColumn col in dt.Columns)
            {
                Debug.Print(col.Caption);
            }

            Debug.Print(dt.Columns["IPSubnet"].Caption);
            Debug.Print(dt.Columns["IPFirst"].Caption);
            Debug.Print(dt.Columns["IPLast"].Caption);

            IpResolver ipResolver = new IpResolver();
            foreach (DataRow row in dt.Rows)
            {
                string subnet = row["IPSubnet"].ToString();
                row["IPFirst"] = ipResolver.GetFirstUsable(subnet);
                row["IPLast"] = ipResolver.GetLastUsable(subnet);

                Console.WriteLine("{0} | {1} | {2}", row["IPSubnet"], row["IPFirst"], row["IPLast"]);

            }

            Console.WriteLine("read {0} lines", dt.Rows.Count);
            Console.WriteLine("column count={0}", dt.Columns.Count);
            foreach (var col in dt.Columns)
            {
                Console.Write(col + "|");
            }
            Console.WriteLine();

            DataRow drow;
            drow = dt.Rows[2];
            Console.WriteLine(drow[0] + "|" + drow[1] + "|" + drow[2]);

            return dt;
        }

        public static DataTable GetOvrnvtsTable()
        {
            Console.WriteLine("processing table SubNetworks...");

            SqlH sqlH = new SqlH(config);

            // SqlConnection conn = sqlH.NewConnection();
            string connection = sqlH.GetBuildConnectionString();
            SqlConnection conn = new SqlConnection(connection);

            if (conn == null)
            {
                return null;
            }

            Debug.Print(conn.ConnectionString);
            Debug.Print("connection state=" + conn.State.ToString());

            SqlCommand sqlcmd = new SqlCommand();
            sqlcmd.Connection = conn;
            sqlcmd.CommandType = CommandType.StoredProcedure;
            sqlcmd.CommandText = "dbo.uspMonthlyReport";

            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter = new SqlParameter("@Param1", "overridenvts");
            sqlcmd.Parameters.Add(sqlParameter);
            sqlcmd.CommandTimeout = 300;

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            sqlDataAdapter.SelectCommand =  sqlcmd;

            DataSet dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);

            Debug.Print("connection state=" + conn.State.ToString());

            if(conn.State == ConnectionState.Open)
                conn.Close();

            DataTable dt = dataSet.Tables[0];

            return dt;
        }

    }
}
