using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UniverServer
{
    class ServerMain
    {
        bool exit = false;
        static MainWindow UI;
        // Monitor
        public static Object monitorLock = new Object();
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
        public String serverIPLocalv4;
        public String serverIPLocalv6;
        public String hostName;
        public int serverPort = 8083;
        public String status = "Offline";
        private List<Task> Threads = new List<Task>();

        private void ProcessClientRequests(TcpClient clientRequest, MainWindow UI)
        {
            try
            {
                StreamReader reader = new StreamReader(clientRequest.GetStream());
                StreamWriter writer = new StreamWriter(clientRequest.GetStream());
                String message = "";
                while (!(message = reader.ReadLine()).Equals("Exit") || (message == null))
                {
                    UI.SetLog("from client: " + message);
                    UI.SetLog("From server: " + message);
                    writer.Flush();
                }
                writer.Close();
                reader.Close();
                clientRequest.Close();
                
            }
            catch(Exception e)
            {

            }
                    
        }


        public void Run(MainWindow ptr)
        {
            for (int i = 0; i <MAX_CLIENTS; i++){
                socketDataBuffer[i] = new byte[BUFFER_SIZE];
            }
            UI = ptr;
            hostName = Dns.GetHostName();
            serverIPLocalv4 = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            serverIPLocalv6 = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            TcpListener listener = new TcpListener(IPAddress.Parse(serverIPLocalv4), serverPort); ;

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
            serverSocket.Listen(MAX_CLIENTS);
            //serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            try
            {
                status = "Online";
                UI.Refresh_Async();
                UI.SetLog("Welcome back, Commander");
                listener.Start();
                UI.SetLog("Listener active");


                status = "Online";
                UI.Refresh_Async();

                while (exit != true)
                {
                    if (MAX_CLIENTS >= receptors)
                    {


                        // TcpClient tcpClient = listener.AcceptTcpClient();
                        //UI.SetLog("Accepted new client");

                        //Threads.Add(new Task(()=>this.ProcessClientRequests(tcpClient, UI)));


                        this.serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);
                        //Threads.Add(new Task(() => this.serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null)));
                        receptors++;

                    }
                }
                //ptr.AddClient("192.77.77.77", "abcd.12f1.2a21.ffa2");
            }
            catch(Exception e)
            {
                ptr.SetLog(e.ToString());
            }
            finally
            {
                listener.Stop();
                UI.SetLog("Exiting...");
                status = "Offline";
                UI.SetLog("server shutdown");
            }

            return;
        }
     

        private void AcceptCallback(IAsyncResult callback)
        {
           // UI.Cli_Mes.Text = $"new Client {Clients.Count}";
           
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
                Socket socket = (Socket)callback.AsyncState;
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
            catch(SocketException e)
            {
                // client disconnect possibly
            }

        }
        private void SendText(String text, Socket socket)
        {
            try
            {
                byte[] dataReply = Encoding.ASCII.GetBytes("0#"+text);
                socket.BeginSend(dataReply, 0, dataReply.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            }
            catch (SocketException) { }
        }
        private void SendCallback(IAsyncResult callback) {
            Socket socket = (Socket)callback.AsyncState;
            socket.EndSend(callback);
            socket.Disconnect(true);
        }

        public void End()
        {
            this.exit = true;
        }
        
    }
}
