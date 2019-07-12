using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using SharedVars;


namespace UniverServer
{
    public class ServerMain
    {
        
        bool exit = false;
        static MainWindow serverMainWindow;
        // Monitor
        public static object monitorLock = new object();
        public static object monitorLockClients = new object();
        // sockets / clients
        int receptors = 0; //number of async callback methods actively awaiting clients
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static public List<Socket> ClientSockets = new List<Socket>();
        static public List<ClientData> Clients = new List<ClientData>();
        static public List<ClientData> ClientHistory = new List<ClientData>();
        byte[][] socketDataBuffer = new byte[Vars.MAX_CLIENTS][];
        
        //network
        public IPAddress serverIPLocalv4;
        public string hostName;
        IPHostEntry hostEntry;

        

        public string status = "Offline";

        public void Run(MainWindow mainWindow)
        {
            for (int i = 0; i < Vars.MAX_CLIENTS; i++){
                socketDataBuffer[i] = new byte[Vars.BUFFER_SIZE];
            }

            ServerMain.serverMainWindow = mainWindow;

            hostName = Dns.GetHostName();
            hostEntry = Dns.GetHostEntry(hostName);
            serverIPLocalv4 = hostEntry.AddressList.Last().MapToIPv4();

            serverSocket.Bind(new IPEndPoint(serverIPLocalv4, Vars.SERVER_PORT));
            serverSocket.Listen(Vars.MAX_CLIENTS);
            
            try
            {
                status = "Online";

                ServerMain.serverMainWindow.Refresh_Async();
                ServerMain.serverMainWindow.SetLog("Welcome back, Commander");


                ServerMain.serverMainWindow.SetLog("Listener active");
                status = "Online";

                ServerMain.serverMainWindow.Refresh_Async();

                while (exit != true)
                {
                    if (Vars.MAX_CLIENTS >= receptors)
                    {
                        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);
                        Interlocked.Increment(ref receptors);
                    }
                }
            }
            catch(Exception e)
            {
                mainWindow.SetLog(e.ToString());
            }
            finally
            {
                ServerMain.serverMainWindow.SetLog("Exiting...");
                status = "Offline";
                ServerMain.serverMainWindow.SetLog("server shutdown");
            }
        }

        private void AcceptCallback(IAsyncResult callback)
        {
            int id;
            Socket socket = serverSocket.EndAccept(callback);
            Monitor.Enter(monitorLockClients);
            Clients.Add(new ClientData(socket));
            ClientSockets.Add(socket);
            id = Clients.Count - 1;
            Monitor.Exit(monitorLockClients);

            Clients[id].thread = Thread.CurrentThread;
            

            var state = new ValueTuple<Socket, int>(socket, id);

            socket.BeginReceive(
                socketDataBuffer[id],
                0,
                socketDataBuffer.Length,
                SocketFlags.None,
                new AsyncCallback(RecieveCallback),
                state);
            Interlocked.Decrement(ref receptors);
            
        }

        private void RecieveCallback(IAsyncResult callback)
        {
            try
            {
                var state = (ValueTuple<Socket, int>)callback.AsyncState;
                var socket = state.Item1;
                int recievedSize = socket.EndReceive(callback);
                byte[] dataBuffer = new byte[recievedSize];

                Array.Copy(socketDataBuffer[state.Item2], dataBuffer, recievedSize);

                string text = Encoding.ASCII.GetString(dataBuffer);
                if (recievedSize > 0)
                {
                    serverMainWindow.SetLog(text);
                    if (text.ToLower() == Vars.CLIENT_SIGN + "wsup")
                    {
                        SendText("All good.", socket);
                        serverMainWindow.SetLog("All good.");
                    }
                    else
                    {
                        //SendText("No Good", socket); //the good reply is overwritten by this
                        serverMainWindow.SetLog("No Good");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SendText(string text, Socket socket)
        {
            try
            {
                byte[] dataReply = Encoding.ASCII.GetBytes(Vars.SERVER_SIGN + text);
                socket.Send(dataReply);
            }
            catch (SocketException e) {
                Console.WriteLine(e);
            }
        }

        public void End() => exit = true;
    }
}
