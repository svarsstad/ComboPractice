using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedVars;

namespace Client
{
    class ClientMain
    {
        MainWindow clientMainWindow;
        bool exit = false;
        public IPAddress serverIPLocalv4;
        public string hostName;
        bool connected = false;
        IPHostEntry hostEntry;
        byte[] Databuffer = new byte[Vars.BUFFER_SIZE];
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public void Run(MainWindow CMW)
        {
            clientMainWindow = CMW;
            hostName = Dns.GetHostName();
            hostEntry = Dns.GetHostEntry(hostName);
            serverIPLocalv4 = hostEntry.AddressList.Last().MapToIPv4();



            try
            {
                //socket.Connect(serverIPLocalv4, Vars.serverPort);
                socket.BeginConnect(serverIPLocalv4, Vars.SERVER_PORT, ConnectionAccepted, null);

                while (!connected)

                {
                    Thread.Sleep(10); 
                }
            }
            catch (SocketException)
            {
                clientMainWindow.Set_Sys_Mes("Socket exception");  // you can put a counter here to see how many attempts are made

            }
            clientMainWindow.Set_Sys_Mes("Connected");
            while (!exit)
            {
                try
                {
                    socket.BeginReceive(Databuffer, 0, Databuffer.Length, SocketFlags.None, RecieveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void ConnectionAccepted(IAsyncResult callback)
        {
            connected = true;
            clientMainWindow.Set_Sys_Mes("Connection Success.");

        }
        private void RecieveCallback(IAsyncResult callback)
        {
            try
            {
                int recievedSize = socket.EndReceive(callback);

                Span<byte> buffer = stackalloc byte[recievedSize];
                buffer = Databuffer;

                string text = Encoding.ASCII.GetString(buffer.ToArray());
                if (recievedSize > 0)
                {
                    clientMainWindow.Set_Sys_Mes(text);
                    if (text.ToLower().StartsWith(Vars.SERVER_SIGN))
                    {
                        clientMainWindow.Set_Sys_Mes(text);
                    }
                    else
                    {
                        // returned packet from a client ; ignore
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Send(string text)
        {
            Span<byte> buffer = stackalloc byte[text.Length + 4]; 
            buffer = Encoding.ASCII.GetBytes(Vars.CLIENT_SIGN + text);

            if (!socket.Connected)
            {
                socket.BeginConnect(
                    serverIPLocalv4,
                    Vars.SERVER_PORT,
                    ConnectionAccepted,
                    null);
                while (!connected)

                {
                    Thread.Sleep(10);
                }
            }

            socket.Send(buffer.ToArray());
        }
    }
}
