using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SQLite;

namespace NRSoft.FunctionPool
{

    public struct Bearbeiter
    {
        public int BearbeiterID;
        public string Name;
        public string Login;
        public string Pwd;
        public bool Change;
        public string Comment;
        public bool IsAdmin;
        public bool IsActive;
    }

    class SQLiteH
    {

        #region fields
        // Declartion internal variables
        private SQLiteConnection m_connection = null;
        private SQLiteDataAdapter m_dataAdapter = null;
        private DataSet m_dataSet = null;
        private string m_connectionString = "";
        private string m_dataSource = "";
        private string m_fieldNameID = "";
        private int m_errornumber = 0;
        //private string m_errordescription = "";
        #endregion fields

        #region Properties
        public static string ConnectionStringSQLite
        {
            get
            {
                string database =
                    //AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Database\\Contact.s3db";
                    AppDomain.CurrentDomain.BaseDirectory + @"\Database\Contact.s3db";
                string connectionString =
                    @"Data Source=" + Path.GetFullPath(database);
                return connectionString;
            }
        }

        public int ErrorNumber
        {
            get { return m_errornumber; }
        }

       public string ConnectionString
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;
            }
        }

        public DataSet DataSet
        {
            get { return m_dataSet; }
        }


        #endregion

        #region CTOR
        // Constructor -> ConnectionString is required
        public SQLiteH()
        { }

        public SQLiteH(string connectionString)
        {
            m_dataSource = connectionString;
            m_connectionString = connectionString;
        }
        #endregion

        #region public methods
        // The DataSet is filled with the methode LoadDataSet

        public bool CloseConnection()
        {
        if (m_connection != null)
            {
            m_connection.Close();
            }
        return true;
        }

        public bool OpenConnection(string connectionString)
        {
            m_dataSource = connectionString;
            bool Result = OpenConnection();
            return Result;
        }

        public bool OpenConnection()
        {
            m_errornumber = 1;

            if (m_dataSource == String.Empty)
            {
                m_errornumber = 1;
                return false;
            }

            m_connection = new SQLiteConnection();
            m_connection.ConnectionString = "Data Source=" + m_dataSource;
            try
            {
                // Open de connectie
                m_connection.Open();
                m_errornumber = 0;
                return true;
            }
            catch (Exception)
            {
                m_errornumber = 1;
                return true;
                throw;
            }
        }
    
        // Load the DataSet 
        public bool Load(string commandText, string fieldNameID)
        {
            // Save the variables
            m_fieldNameID = fieldNameID;

            try
            {
                // Open de connectie
                m_connection = new SQLiteConnection(m_dataSource);
                m_connection.Open();

                // Maak een DataAdapter
                m_dataAdapter = new SQLiteDataAdapter(commandText, m_connection);

                // Koppel een eventhandler aan het RowUpdated-event van de DataAdapter
                //m_dataAdapter.RowUpdated += new SqlRowUpdatedEventHandler(m_dataAdapter_RowUpdated);
                m_dataAdapter.RowUpdated += m_dataAdapter_RowUpdated;
                m_dataSet = new DataSet();

                // Voor eventueel opslaan --> Commands maken
                if (!string.IsNullOrEmpty(fieldNameID))
                    {
                    SQLiteCommandBuilder commandBuilder = new SQLiteCommandBuilder(m_dataAdapter);
                    m_dataAdapter.InsertCommand = commandBuilder.GetInsertCommand();
                    m_dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand();
                    m_dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                    }

                // Vul de DataSet
                m_dataAdapter.Fill(m_dataSet);

                // We zijn hier, alles okay!
                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Altijd netjes sluiten
                m_connection.Close();
            }
        }

        // Laad de DataSet
        public bool Load(string commandText)
        {
            return Load(commandText, "");
        }

        // Sla de DataSet op
        public bool Save()
        {
            // Hij kan alleen gegevens opslaan in ID bekend is
            if (m_fieldNameID.Trim().Length == 0)
            {
                return false;
            }

            try
            {
                // Open de connectie
                m_connection.Open();

                // Sla de DataRow op. Dit vuurt het event OnRowUpdated af
                m_dataAdapter.Update(m_dataSet);

                // We zijn hier, alles okay!
                return true;
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Altijd netjes sluiten
                m_connection.Close();
            }
        }

        // Rij is opgeslagen, bepaal eventueel de nieuwe ID
        void m_dataAdapter_RowUpdated(object sender, System.Data.Common.RowUpdatedEventArgs e)
        {
            // Het (zojuist verkregen?) ID is alleen interessant bij een nieuwe record
            if (e.StatementType == StatementType.Insert)
            {
                // Bepaal het zojuist verkregen ID
                //SQLiteCommand command = new SQLiteCommand("SELECT @@IDENTITY", m_connection);
                SQLiteCommand command = new SQLiteCommand("SELECT last_insert_rowid() AS ID", m_connection);

                // Bepaal de nieuwe ID en sla deze op in het juiste veld
                object nieuweID = command.ExecuteScalar();

                // Bij evt. fouten geen ID --> Daarom testen
                if (nieuweID == System.DBNull.Value == false)
                {
                    // Zet de ID in de juiste kolom in de DataRow
                    e.Row[m_fieldNameID] = Convert.ToInt32(nieuweID);
                }
            }
        }

        public DataTable GetDataTable(string sql)
            {
            DataTable dt = new DataTable();
            try
                {

                // SQLiteConnection cnn = new SQLiteConnection(m_connection);
                // cnn.Open();
                OpenConnection();
                SQLiteCommand mycommand = new SQLiteCommand(m_connection);
                mycommand.CommandText = sql;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                CloseConnection();
                }
            catch (Exception e)
                {
                throw new Exception(e.Message);
                }
            return dt;
            }

        // returns a single record bearbeiter
        public Bearbeiter GetListByCommand(string m_command)
            {
            Bearbeiter m_Record = new Bearbeiter();

            try
                {
                OpenConnection();
                using (SQLiteCommand cmd = new SQLiteCommand(m_connection))
                    {
                    cmd.CommandText = m_command;
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                        string id = "";
                        while (rdr.Read())
                            {
                            id = rdr["id"].ToString();
                            m_Record.BearbeiterID = Convert.ToInt16(id);
                            m_Record.Name = rdr["name"].ToString();
                            m_Record.Login = rdr["login"].ToString();
                            m_Record.Pwd = rdr["pwd"].ToString();
                            m_Record.Change = Convert.ToBoolean( rdr["change"].ToString() );
                            m_Record.Comment = rdr["comment"].ToString();
                            m_Record.IsAdmin = Convert.ToBoolean( rdr["isadmin"].ToString() );
                            m_Record.IsActive = Convert.ToBoolean(rdr["isactive"].ToString());
                            }
                        }
                    // CloseConnection();
                    return m_Record;
                    }
                }
            finally
                {
                if (m_connection != null)
                    m_connection.Dispose();
                }
            }

        // returns a single record bearbeiter
        public List<Bearbeiter> GetBearbeiterList(string m_command)
            {

            Bearbeiter m_Record = new Bearbeiter();
            List<Bearbeiter> lb = new List<Bearbeiter>();

            try
                {
                OpenConnection();
                using (SQLiteCommand cmd = new SQLiteCommand(m_connection))
                    {
                    cmd.CommandText = m_command;
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                        string id = "";
                        while (rdr.Read())
                            {
                            id = rdr["id"].ToString();
                            m_Record.BearbeiterID = Convert.ToInt16(id);
                            m_Record.Name = rdr["name"].ToString();
                            m_Record.Login = rdr["login"].ToString();
                            m_Record.Pwd = rdr["pwd"].ToString();
                            m_Record.Change = Convert.ToBoolean(rdr["change"].ToString());
                            m_Record.Comment = rdr["comment"].ToString();
                            m_Record.IsAdmin = Convert.ToBoolean(rdr["isadmin"].ToString());
                            m_Record.IsActive = Convert.ToBoolean(rdr["isactive"].ToString());
                            lb.Add(m_Record);
                            }
                        }
                    // CloseConnection();

                    Console.WriteLine(lb.Count());

                    return lb;
                    }
                }
            finally
                {
                if (m_connection != null)
                    m_connection.Dispose();
                }
            }
        #endregion public methods

    }
}
