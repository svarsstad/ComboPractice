using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniverServer
{
    class ServerMain
    {
        bool exit = false;
        static MainWindow UI;
        // Monitor
        public static object monitorLock = new object();
        // sockets / clients
        int receptors = 0; //number of async callback methods actively awaiting clients
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static public List<Socket> ClientSockets = new List<Socket>();
        static public List<ClientData> Clients = new List<ClientData>();
        static public List<ClientData> ClientHistory = new List<ClientData>();
        const int MAX_CLIENTS = 10;
        const int BUFFER_SIZE = 128;
        byte[][] socketDataBuffer = new byte[MAX_CLIENTS][];
       
        
        //network
        public string serverIPLocalv4;
        public string serverIPLocalv6;
        public string hostName;
        public int serverPort = 8083;
        public string status = "Offline";

        private void ProcessClientRequests(TcpClient clientRequest, MainWindow UI)
        {
            try
            {
                using (var reader = new StreamReader(clientRequest.GetStream()))
                using (var writer = new StreamWriter(clientRequest.GetStream()))
                {
                    var message = "";
                    while (!(message = reader.ReadLine()).Equals("Exit") || (message == null))
                    {
                        UI.SetLog("from client: " + message);
                        UI.SetLog("From server: " + message);
                        writer.Flush();
                    }
                    writer.Close();
                    reader.Close();
                }
                clientRequest.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Run(MainWindow mainWindow)
        {
            for (int i = 0; i < MAX_CLIENTS; i++){
                socketDataBuffer[i] = new byte[BUFFER_SIZE];
            }

            UI = mainWindow;
            hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);
            serverIPLocalv4 = hostEntry.AddressList[1].ToString();
            serverIPLocalv6 = hostEntry.AddressList[0].ToString();
            var listener = new TcpListener(IPAddress.Parse(serverIPLocalv4), serverPort);

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
            serverSocket.Listen(MAX_CLIENTS);
            
            try
            {
                status = "Online";

                UI.RefreshAll();
                UI.SetLog("Welcome back, Commander");

                listener.Start();

                UI.SetLog("Listener active");
                status = "Online";

                UI.RefreshAll();

                while (exit != true)
                {
                    if (MAX_CLIENTS >= receptors)
                    {
                        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);
                        receptors++;
                    }
                }
            }
            catch(Exception e)
            {
                mainWindow.SetLog(e.ToString());
            }
            finally
            {
                listener.Stop();
                UI.SetLog("Exiting...");
                status = "Offline";
                UI.SetLog("server shutdown");
            }
        }

        private void AcceptCallback(IAsyncResult callback)
        {
            Socket socket = serverSocket.EndAccept(callback);
            Clients.Add(new ClientData(socket));
            Clients[Clients.Count - 1].thread = Thread.CurrentThread;
            ClientSockets.Add(socket);
            socket.BeginReceive(socketDataBuffer[Clients.Count - 1], 0, socketDataBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            receptors--;
        }

        private void RecieveCallback(IAsyncResult callback)
        {
            try
            {
                var socket = (Socket)callback.AsyncState;
                int recievedSize = socket.EndReceive(callback);
                byte[] dataBuffer = new byte[recievedSize];

                Array.Copy(socketDataBuffer[1], dataBuffer, recievedSize);

                string text = Encoding.ASCII.GetString(dataBuffer);
                if (text[0] == '1')
                {
                    if (recievedSize > 0)
                    {
                        UI.SetLog(text);
                        if (text.ToLower() == "1#wsup")
                        {
                            SendText("All good.", socket);
                            UI.SetLog("All good.");
                        }
                        else
                        {
                            SendText("No Good", socket);
                            UI.SetLog("No Good");
                        }
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
                byte[] dataReply = Encoding.ASCII.GetBytes("0#"+text);
                socket.BeginSend(dataReply, 0, dataReply.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            }
            catch (SocketException e) {
                Console.WriteLine(e);
            }
        }

        private void SendCallback(IAsyncResult callback) {
            var socket = (Socket)callback.AsyncState;
            socket.EndSend(callback);
            socket.Disconnect(true);
        }

        public void End()
        {
            exit = true;
        }
    }
}
