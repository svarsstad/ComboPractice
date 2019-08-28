using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

namespace UniverServer

{
    public class DatabaseManager : IDisposable
    {
        String connectString = ConfigurationManager.ConnectionStrings["UniverServer.Properties.Settings.DatabaseConnectionString"].ConnectionString;
        private MainWindow mainWindow;
        private SqlConnection sqlConn;
        private SqlCommand sqlComm;
        private bool set_up = false;
        public static object monitorDBLock = new object();
        private String[] Tables = { "PlayerTable", "Areas" };
        private String[][] Columns;

        private void setup()
        {

            sqlConn = new SqlConnection(connectString);
            Columns = new string[Tables.Length][];
            string[] restrictions = new string[4] { null, null, null, null };
            Array columnList;
            try
            {

                sqlConn.Open();
                for (int tableIndex = 0; tableIndex < Tables.Length; tableIndex++)
                {
                    restrictions[2] = Tables[tableIndex];
                    columnList = sqlConn.GetSchema("Columns", restrictions).AsEnumerable().Select(s => s.Field<String>("Column_Name")).ToArray();
                    Columns[tableIndex] = new string[columnList.Length];
                    Array.Copy(columnList, Columns[tableIndex], columnList.Length);
                }

                mainWindow.SetLog("DataBase Assimulated");
            }
            catch (System.Exception ex)
            {
                mainWindow.SetLog(ex.ToString());
            }
            finally
            {
                //sqlConn.Close();
            }
            set_up = true;
        }

        private bool GetSessionId(string username, string password, out int sessionId)
        {
            int table = 0;
            int passwordColumn = 7;
            int usernameColumn = 6;
            System.Text.StringBuilder SB = new StringBuilder();
            SB.Append("SELECT ");
            SB.Append("sessionId");
            SB.Append(" FROM ");
            SB.Append(Tables[table]);
            SB.Append(" WHERE ");
            SB.Append(Columns[table][usernameColumn]);
            SB.Append(" = ");
            SB.Append(username.ToString());
            SB.Append(" AND ");
            SB.Append(Columns[table][passwordColumn]);
            SB.Append(" = ");
            SB.Append(password.ToString());
            using (sqlConn)
            {
                sqlConn.Open();
                sqlComm = new SqlCommand(SB.ToString(), sqlConn);
                Object result = sqlComm.BeginExecuteReader();
                sessionId = (int)result;

                sqlConn.Close();
            }


            return false;
        }

        public int Login(string username, string password)
        {
            int sessionId;
            if (GetSessionId(username, password, out sessionId))
            {
                return sessionId;
            }


            else
            {
                return -1;
            }
        }

        public Action EndSession(int id)
        {
            int table = 0;
            int column = 5;
            Span<byte> value = stackalloc byte[4]; // 2 chars X 2 bytes
            value = Encoding.ASCII.GetBytes("-1");
            SQLSetData(table, column, id, value);
            return null;
        }

        public Action EndSessions()
        {
            int table = 0;
            int column = 5;
            Span<byte> value = stackalloc byte[4]; // 2 chars X 2 bytes
            value = Encoding.ASCII.GetBytes("-1");
            SQLSetData(table, column, null, value);
            return null;
        }

        public Action SetSession(int id, int clientId)
        {
            int table = 0;
            int column = 2; //figure this stuff out
            int textByteSize = ((int)Math.Floor(Math.Log10(clientId) + 1)) * 2;
            Span<byte> value = stackalloc byte[textByteSize];
            value = Encoding.ASCII.GetBytes(clientId.ToString());
            SQLSetData(table, column, id, value);
            return null;
        }

        public Action SQLSetData(int table, int column, Object index, Span<byte> value)
        {
            return null;
            ///pass null as index to change the whole row
            System.Text.StringBuilder SB = new StringBuilder();
            SB.Append("UPDATE ");
            SB.Append(Tables[table]);
            SB.Append(" SET ");
            SB.Append(Columns[table][column]);
            SB.Append(" = ");
            SB.Append(value.ToString());
            if (index != null)
            {
                SB.Append(" WHERE ID = ");
                SB.Append(index);
            }
            if (set_up)
            {
                using (sqlConn)
                {
                    sqlComm = new SqlCommand(SB.ToString(), sqlConn);
                    string retval = "rows affected" + sqlComm.ExecuteNonQuery();
                    mainWindow.SetLog(retval);
                    //sqlConn.Close();
                }
            }
            return null;
        }

        public Action SQLInsertData(int table, List<string> values)
        {
            System.Text.StringBuilder SB = new StringBuilder();
            SB.Append("INSERT INTO ");
            SB.Append(Tables[table]);
            SB.Append(" VALUES(");
            for (int i = 0; i < values.Count() - 1; i++)
            {
                SB.Append(values[i] + ", ");
            }
            SB.Append(values[values.Count()] + ")");
            using (sqlConn)
            {
                sqlConn.Open();
                sqlComm = new SqlCommand(SB.ToString(), sqlConn);
                mainWindow.SetLog("rows affected" + sqlComm.ExecuteNonQuery());
                sqlConn.Close();
            }
            return null;
        }

        public Action Run(MainWindow mainWindowP, CancellationToken BacklineCanselToken)
        {
            mainWindow = mainWindowP;
            setup();
            while (!BacklineCanselToken.IsCancellationRequested)
            {
                //Thread.Sleep(1);
            }

            Dispose();
            return null;
        }

        public void Dispose()
        {
            if (sqlConn.State != System.Data.ConnectionState.Closed)
            {
                sqlConn.Close();
            }
            set_up = false;
        }
    }
}