﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SharedVars;


namespace UniverServer
{
    public class ServerMain
    {
        
        static bool exit = false;
        static MainWindow serverMainWindow;
        // Monitor
        public static object monitorLock = new object();
        public static object monitorLockClients = new object();
        // sockets / clients
        int receptors = 0; //number of async callback methods actively awaiting clients
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //static public List<Socket> ClientSockets = new List<Socket>();
        static public List<ClientData> Clients = new List<ClientData>();
        static public List<ClientData> ClientHistory = new List<ClientData>();
        byte[][] socketDataBuffer = new byte[Vars.MAX_CLIENTS][];
        
        //network
        public IPAddress serverIPLocalv4;
        public string hostName;
        IPHostEntry hostEntry;


       
        public string status = "Offline";

        public void Run(MainWindow mainWindow, CancellationToken BacklineCanselToken)
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

                status = "Online";

                ServerMain.serverMainWindow.Refresh_Async();

                while (exit != true && !BacklineCanselToken.IsCancellationRequested)
                {
                    if (Vars.MAX_CLIENTS >= receptors)
                    {
                        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);
                        Interlocked.Increment(ref receptors);
                    }
                    Monitor.Enter(monitorLock);
                    for( int i = 0;  i < Clients.Count-1; i ++)
                    {
                        if (!Clients[i].socket.Connected)
                        {
                            Clients[i].End();
                            Clients.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            Clients[i].i = i;
                        }
                    }

                    Monitor.Exit(monitorLock);
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
            id = Clients.Count;
            Clients.Add(new ClientData(id, socket));
            Monitor.Exit(monitorLockClients);

            Clients[id].task = new Task(ClientHandle(Clients[id]));
            Interlocked.Decrement(ref receptors);
            

            //var state = new ValueTuple<Socket, int>(socket, id);

            
            
        }

        private Action ClientHandle(ClientData clientData)
        {
            while (!exit)
            {
                try
                {
                   
                    if (clientData.dataToSend) {
                    //    clientData.socket.BeginSend();
                            }

                    clientData.socket.BeginReceive(
                        socketDataBuffer[clientData.i],
                        0,
                        socketDataBuffer[clientData.i].Length,
                        SocketFlags.None,
                        new AsyncCallback(RecieveCallback),
                        clientData);
                } catch(Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            return null;
        }



        public void SendTextAll(string text)
        {
            Task.Run(() =>
            {
                text = Vars.SERVER_SIGN + text;
                Span<byte> dataReply = stackalloc byte[text.Length * 2];
                dataReply = Encoding.ASCII.GetBytes(text);
                for (int i = 0; i < Clients.Count-1; i++)
                {
                    try
                    {
                        Clients[i].socket.Send(dataReply.ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }
        public void SendText(string text, int clientIndex)
        {
            Task.Run(() =>
            {
                text = Vars.SERVER_SIGN + text;
                Span<byte> dataReply = stackalloc byte[text.Length * 2];
                dataReply = Encoding.ASCII.GetBytes(text);

                try
                {
                    Clients[clientIndex].socket.Send(dataReply.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            );
        }

        private void RecieveCallback(IAsyncResult callback)
        {
            try
            {
                var clientData = (ClientData)callback.AsyncState;
                
                int recievedSize = clientData.socket.EndReceive(callback);
                byte[] dataBuffer = new byte[recievedSize];

                Array.Copy(socketDataBuffer[clientData.i], dataBuffer, recievedSize);

                string text = Encoding.ASCII.GetString(dataBuffer);
                if (recievedSize > 0)
                {
                    serverMainWindow.SetLog(text);
                    if (text.ToLower() == Vars.CLIENT_SIGN + "wsup")
                    {
                        SendText("All good.", clientData.socket);
                        serverMainWindow.SetLog("All good.");
                    }
                    else if(text == "~")
                    {
                        int i = clientData.i;
                        
                        clientData.End();
                        Clients.RemoveAt(i);
                    }
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

        private void SendText (string text, Socket socket)
        {

            try
            {
                text = Vars.SERVER_SIGN + text;
                Span<byte> dataReply = stackalloc byte[text.Length * 2];
                dataReply = Encoding.ASCII.GetBytes(text);
                socket.Send(dataReply.ToArray());
            }
            catch (SocketException e) {
                Console.WriteLine(e);
            }
        }
        ~ServerMain()
        {
            End();
        }
        public void End()
        {
            exit = true;
            for (int i = 0; i < Clients.Count-1; i++)
            {
                Clients[i].End();
            }
        }
    }
}
