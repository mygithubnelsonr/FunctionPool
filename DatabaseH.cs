using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;


namespace NRSoft.FunctionPool
{
    public class DatabaseH
    {
        public enum dbType
        {
            oledb = 0,
            sqlite = 1,
            mssql = 2,
            odbc = 3,
        }

        #region private fields
        private int _dbType;
        private string _dataSource;
        private bool _connEstablished;
        private string _connectionString;
        private DataSet _dataSet = null;
        private OleDbConnection _connection = null;

        // private string _dbLocation;
        // private SQLiteConnection m_connection = null;
        //private SQLiteDataAdapter m_dataAdapter = null;
        #endregion

        #region properties

        public int DatabaseType
        {
            get { return _dbType; }
            set { _dbType = value;  }
        }
        public bool ConnectionEstablished
        {
            get { return _connEstablished; }
            set { _connEstablished = value; }
        }

        public string Datasource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                _connectionString += _dataSource;
            }
        }

        public string ConnectionSting
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public SqlConnection SQLConnection()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            return conn;
        }

        public OleDbConnection Connection
        {
            get { return _connection; }
        }

        // The DataSet is filled with the methode LoadDataSet
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        #endregion properties

        #region ctor
        // Constructor -> ConnectionString is required
        public DatabaseH(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DatabaseH()
        {
            _connectionString = "";
            _dataSource = "";
        }

        #endregion

        #region methods

        public string QueryBuilder(string tableName)
        {
            string lg = "", lk = "", strAnd, strQuery, strTemp, strResult, strGenre, strKatalog, strAlbum, strInterpret;

            Hashtable ht = new Hashtable();
            RegistryH rh = new RegistryH("NRSoft", "MyJukebox.net");

            // (letzter)selectierte genre, sollte nur beim erststart 'alle' sein
            lg = rh.GetSetting(@"Settings\Logical\Genre", "LastGenre", "Alle");
            strGenre = "";
            if (lg != "Alle")
            {
                ht.Add("genre", lg);
                strGenre = " genre LIKE '" + lg + "'";
            }
            //  (letzter)selectierter katalog
            lk = rh.GetSetting(@"Settings\Logical\Genre\" + lg, "LastKatalog", "Alle");
            strKatalog = "";
            if (lk != "Alle")
            {
                ht.Add("katalog", lk);
                strKatalog = " katalog LIKE '" + lk + "'";
            }

            string ls = rh.GetSetting(@"Settings\Logical\Katalog\" + lk, "LastSelect", "Album");
            if (ls == "Album")
            {
                // selectiertes album
                strResult = rh.GetSetting(@"Settings\Logical\Katalog\" + lk, "LastAlbum", "Alle");
                strAlbum = "";
                if (strResult != "Alle")
                {
                    ht.Add("album", strResult);
                    strAlbum = " album LIKE '" + strResult.Replace("'", "''") + "'";
                }
            }

            if (ls == "Interpret")
            {
                //  selectierter interpret
                strResult = rh.GetSetting(@"Settings\Logical\Katalog\" + lk, "LastInterpret", "Alle");
                strInterpret = "";
                if (strResult != "Alle")
                {
                    ht.Add("interpret", strResult);
                    strInterpret = " interpret LIKE '" + strResult.Replace("'", "''") + "'";
                }
            }

            strTemp = "";
            strQuery = "";
            strAnd = "";
            strQuery = "";

            if (ht.Count > 1) strAnd = " AND ";
            int htcount = 1;
            // Query zusammenbasteln
            foreach (DictionaryEntry de in ht)
            {
                string v = de.Value.ToString();
                Console.WriteLine("Key = {0}, Value = {1}", de.Key, v.Replace("'", "''"));
                strTemp = "";
                strTemp += de.Key + " LIKE '" + v.Replace("'", "''") + "'";
                strQuery += " (" + strTemp + ") ";
                if (htcount < ht.Count) strQuery += strAnd;
                htcount++;
            }

            if (ht.Count > 0)   // all keys are 'Alle'
                strQuery = " WHERE " + strQuery;

            strQuery = "SELECT * FROM " + tableName + strQuery;
            return strQuery;
        }

        public bool ExecuteNonQuery(string SQL)
        {
            bool success = false;
            try
            {
                _connection = new OleDbConnection(_connectionString);
                OleDbCommand cmd = new OleDbCommand(SQL, _connection);
                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                success = false;
            }
            return success;
        }

        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                SqlConnection conn = new SqlConnection(_connectionString);
                //OpenConnection();
                SqlCommand command = new SqlCommand(sql, conn);
                conn.Open();
                //mycommand.CommandText = sql;
                SqlDataReader reader = command.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }

        public ArrayList ReadData(string SQL)
        {
            SqlConnection conn;
            using ( conn = SQLConnection())
            {
                SqlCommand cmd = new SqlCommand(SQL, conn);
                try
                {
                    conn.Open();
                }
                catch
                {
                    return null;
                }

                SqlDataReader dr;
                dr = cmd.ExecuteReader();

                ArrayList strarData = new ArrayList();

                while (dr.Read())
                {
                    strarData.Add(dr[0].ToString());
                }

                return strarData;

            }
        }


        #endregion

        // ConnectionString for SQLite
        // public string ConnectionStringSQLite
        //{
        //	get
        //	{
        //		string database =
        //			//AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Database\\Contact.s3db";
        //			AppDomain.CurrentDomain.BaseDirectory + "\\Database\\Contact.s3db";
        //		string connectionString =
        //			@"Data Source=" + Path.GetFullPath(database);
        //		return connectionString;
        //	}
        //}

        //// Load the DataSet 
        ////public bool Load(string commandText, string fieldNameID)
        //{
        //    // Save the variables
        //    m_fieldNameID = fieldNameID;

        //    try
        //    {
        //        // Open de connectie
        //        m_connection = new SQLiteConnection(m_connectionString);
        //        m_connection.Open();

        //        // Maak een DataAdapter
        //        m_dataAdapter = new SQLiteDataAdapter(commandText, m_connection);

        //        // Koppel een eventhandler aan het RowUpdated-event van de DataAdapter
        //        //m_dataAdapter.RowUpdated += new SqlRowUpdatedEventHandler(m_dataAdapter_RowUpdated);
        //        m_dataAdapter.RowUpdated += m_dataAdapter_RowUpdated;
        //        m_dataSet = new DataSet();

        //        // Voor eventueel opslaan --> Commands maken
        //        if (!string.IsNullOrEmpty(fieldNameID))
        //        {
        //            SQLiteCommandBuilder commandBuilder = new SQLiteCommandBuilder(m_dataAdapter);
        //            m_dataAdapter.InsertCommand = commandBuilder.GetInsertCommand();
        //            m_dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand();
        //            m_dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand();
        //        }

        //        // Vul de DataSet
        //        m_dataAdapter.Fill(m_dataSet);

        //        // We zijn hier, alles okay!
        //        return true;
        //    }

        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        // Altijd netjes sluiten
        //        m_connection.Close();
        //    }
        //}

        //// Laad de DataSet
        //public bool Load(string commandText)
        //{
        //    return Load(commandText, "");
        //}

        //// Sla de DataSet op
        //public bool Save()
        //{
        //    // Hij kan alleen gegevens opslaan in ID bekend is
        //    if (m_fieldNameID.Trim().Length == 0)
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        // Open de connectie
        //        m_connection.Open();

        //        // Sla de DataRow op. Dit vuurt het event OnRowUpdated af
        //        m_dataAdapter.Update(m_dataSet);

        //        // We zijn hier, alles okay!
        //        return true;
        //    }

        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        // Altijd netjes sluiten
        //        m_connection.Close();
        //    }
        //}

        //// Rij is opgeslagen, bepaal eventueel de nieuwe ID
        //void m_dataAdapter_RowUpdated(object sender, System.Data.Common.RowUpdatedEventArgs e)
        //{
        //    // Het (zojuist verkregen?) ID is alleen interessant bij een nieuwe record

        //    if (e.StatementType == StatementType.Insert)
        //    {
        //        // Bepaal het zojuist verkregen ID
        //        //SQLiteCommand command = new SQLiteCommand("SELECT @@IDENTITY", m_connection);
        //        SQLiteCommand command = new SQLiteCommand("SELECT last_insert_rowid() AS ID", m_connection);


        //        // Bepaal de nieuwe ID en sla deze op in het juiste veld
        //        object nieuweID = command.ExecuteScalar();

        //        // Bij evt. fouten geen ID --> Daarom testen
        //        if (nieuweID == System.DBNull.Value == false)
        //        {
        //            // Zet de ID in de juiste kolom in de DataRow
        //            e.Row[m_fieldNameID] = Convert.ToInt32(nieuweID);
        //        }
        //    }
        //}

    }
}
