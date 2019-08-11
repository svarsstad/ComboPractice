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

namespace UniverServer
    

{
    public class DatabaseManager
    {
        MainWindow mainWindow;
        SqlConnection sqlConn;
        public static object monitorDBLock = new object();
        String[] Tables = { "PlayerTable", "Areas" };
        void setup()
        {
            String str = ConfigurationManager.ConnectionStrings["UniverServer.Properties.Settings.DatabaseConnectionString"].ConnectionString;
            sqlConn = new SqlConnection(str);
            try
            {
                sqlConn.Open();
                //myCommand.ExecuteNonQuery();
                mainWindow.SetLog("DataBase is Created Successfully");
            }
            catch (System.Exception ex)
            {
                mainWindow.SetLog(ex.ToString());
            }
        }
        public Action SQLSetData(int table, int column,int index,Span<byte> value)
        {
            System.Text.StringBuilder SB = new StringBuilder();
            SB.Append("UPDATE ");
            SB.Append(Tables[table]);
            SB.Append(" WHERE ID");

            SqlCommand comm = new SqlCommand(SB.ToString(), sqlConn);
            mainWindow.SetLog("rows affected" + comm.ExecuteNonQuery());
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