using System.Threading.Tasks;
using System;
using System.Threading;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SharedVars;
using System.Configuration;
using System.Data;

namespace UniverServer
    

{
    public class DatabaseManager
    {
        MainWindow mainWindow;
        SqlConnection sqlConn;
        SqlCommand sqlComm;
        public static object monitorDBLock = new object();
        String[] Tables = { "PlayerTable", "Areas" };
        String[][] Columns;

        void setup()
        {
            String str = ConfigurationManager.ConnectionStrings["UniverServer.Properties.Settings.DatabaseConnectionString"].ConnectionString;
            sqlConn = new SqlConnection(str);
            Columns = new string[Tables.Length][];
            string[] restrictions = new string[4] { null, null, null, null };
            Array columnList;
            try
            {
                using (sqlConn)
                {
                    sqlConn.Open();
                    for (int tableIndex = 0; tableIndex < Tables.Length; tableIndex++) {
                        restrictions[2] = Tables[tableIndex];
                        columnList = sqlConn.GetSchema("Columns", restrictions).AsEnumerable().Select(s => s.Field<String>("Column_Name")).ToArray();
                        Columns[tableIndex] = new string[columnList.Length];
                        Array.Copy(columnList, Columns[tableIndex], columnList.Length);
                    }

                }
                mainWindow.SetLog("DataBase Assimulated");
            }
            catch (System.Exception ex)
            {
                mainWindow.SetLog(ex.ToString());
            }
            finally
            {
                sqlConn.Close();
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
        public Action SetSession(int id,int clientId)
        {
            int table = 0;
            int column = 2; //figure this stuff out
            int textByteSize= ((int)Math.Floor(Math.Log10(clientId) + 1)) * 2;
            Span<byte> value = stackalloc byte[textByteSize];
            value = Encoding.ASCII.GetBytes(clientId.ToString());
            SQLSetData(table, column, id, value);
            return null;
        }
        public Action SQLSetData(int table, int column,int index,Span<byte> value)
        {
            System.Text.StringBuilder SB = new StringBuilder();
            SB.Append("UPDATE ");
            SB.Append(Tables[table]);
            SB.Append(" SET ");
            SB.Append(Columns[table][column]);
            SB.Append(" = ");
            SB.Append(value.ToString());
            SB.Append(" WHERE ID = ");
            SB.Append(index);

            using (sqlConn)
            {
                sqlConn.Open();
                sqlComm = new SqlCommand(SB.ToString(), sqlConn);
                mainWindow.SetLog("rows affected" + sqlComm.ExecuteNonQuery());
                sqlConn.Close();
            }
            return null;
        }

        public Action SQLInsertData(int table, List<string> values)
        {
            System.Text.StringBuilder SB = new StringBuilder();
            SB.Append("INSERT INTO ");
            SB.Append(Tables[table]);
            SB.Append(" VALUES(");
            for(int i = 0; i < values.Count() -1; i++)
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

        public void Run(MainWindow mainWindowP, CancellationToken BacklineCanselToken)
        {
            mainWindow = mainWindowP;
            setup();
            while (!BacklineCanselToken.IsCancellationRequested)
            {
                Thread.Sleep(1);



            }


            End();
        }
        public void End()
        {
            if (sqlConn.State != System.Data.ConnectionState.Closed)
            {
                sqlConn.Close();
            }
        }
    }
}