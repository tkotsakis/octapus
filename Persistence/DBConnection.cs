using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Sybase.Data.AseClient;
using System.Data.Odbc;
using System.Data;

namespace Relational.Octapus.Persistence
{
    public class DBConnection
    {
        private SqlConnection conn;
        private AseConnection aseConn;
        private OdbcConnection odbcConn;
        private SqlCommand cmd;
        private AseCommand aseCmd;
        private OdbcCommand odbcCmd;
        private string connectionString,returnRow;
        private object command;

        public DBConnection(DBMS dbms)
        {
            switch (dbms)
            {
                case DBMS.Sybase:
                    this.aseConn = new AseConnection();
                    command = new AseCommand("", aseConn);
                    break;
                case DBMS.Microsoft:
                    this.conn = new SqlConnection();
                    command = new SqlCommand("", conn);
                    break;
                case DBMS.Odbc:
                    this.odbcConn = new OdbcConnection();
                    command = new OdbcCommand("", odbcConn);
                    break;
                default:
                    break;
            }
        }

        public string ConnectionString
        {
            get { return this.connectionString; }
            set { this.connectionString = value; }
        }

        public bool IsConnected
        {
            get { return this.State == ConnectionState.Open; }
        }

        public ConnectionState State { get; set; }
        public bool FireInfoMessageEventOnUserErrors { get; set; }
        public SqlInfoMessageEventHandler InfoMessage { get; set; }

        public void Open()
        {
            aseConn.ConnectionString = this.connectionString;
            aseConn.Open();
        }

        public void Close()
        {
            aseConn.Close();
        }

        public AseCommand AseCommand { get { return this.aseCmd; } set { } }
        public SqlCommand SqlCommand { get { return this.cmd; } set { } }
        public OdbcCommand OdbcCommand { get { return this.odbcCmd; } set { } }

        public object GetCommand()
        {
            return this.command;
        }


        public DataTable GetDataTable(string sqlCommand, ref string returnMessage)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                returnMessage = "OK";
                AseDataAdapter da = new AseDataAdapter(sqlCommand, this.aseConn);
                da.Fill(ds);

            }
            catch (AseException ex)
            {
                returnMessage = ex.Message;
            }
            return ds.Tables[0];
        }

        public List<string> Parse(string sqlText)
        {
            List<String> ErrMessages = new List<string>();
            try
            {
                var cmd = ((IDbCommand)command);
                cmd.CommandText = "SET NOEXEC ON";
                cmd.ExecuteNonQuery();
                cmd.CommandText = sqlText;
                cmd.ExecuteNonQuery();
                cmd.CommandText = "SET NOEXEC OFF";
                cmd.ExecuteNonQuery();
            }
            catch (AseException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    ErrMessages.Add(ex.Errors[i].Message);
                }

            }
            return ErrMessages;
        }

        public void Execute (string sqlText)
        {
            try
            {
                var cmd = ((IDbCommand)command);
                cmd.CommandText = sqlText;
                if (sqlText.ToUpper().Contains("SELECT")) returnRow = cmd.ExecuteScalar().ToString();
                cmd.ExecuteNonQuery();
            }
            catch (AseException ex)
            {
                throw ex;

            }
        }

        public void Execute(WorkspaceParams workspaceParams, string statement, OctapusLog logger,DBConnection dbConnection)
        {
            dbConnection.ConnectionString = workspaceParams.DBConnectionString;
            dbConnection.Open();
            var errorList = dbConnection.Parse(statement);
            if (errorList.Count == 0)
            {
                dbConnection.Execute(statement);
            }
            else
            {
                logger.LogInfo("Execute: " + errorList[0]);
                dbConnection.Close();
            }
            if (dbConnection.IsConnected) dbConnection.Close();
        }

        public string GetSqlResult()
        {
            return returnRow;
        }
    }
}
