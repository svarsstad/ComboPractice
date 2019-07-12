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
        MainWindow ClientMainWindow;
        bool exit = false;
        public IPAddress serverIPLocalv4;
        public string hostName;
        bool connected = false;
        IPHostEntry hostEntry;
        byte[] Databuffer = new byte[Vars.BUFFER_SIZE];
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public void Run(MainWindow CMW)
        {
            ClientMainWindow = CMW;
            hostName = Dns.GetHostName();
            hostEntry = Dns.GetHostEntry(hostName);
            serverIPLocalv4 = hostEntry.AddressList.Last().MapToIPv4();



            try
            {
                //socket.Connect(serverIPLocalv4, Vars.serverPort);
                socket.BeginConnect(serverIPLocalv4, Vars.serverPort, ConnectionAccepted, null);

                while (!connected)

                {
                    Thread.Sleep(10000); // i'm not sure if this does anything
                }
            }
            catch (SocketException)
            {
                ClientMainWindow.Set_Sys_Mes("Socket exception");  // you can put a counter here to see how many attempts are made

            }
            ClientMainWindow.Set_Sys_Mes("Connected");
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
            ClientMainWindow.Set_Sys_Mes("Connection Success.");

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
                    ClientMainWindow.Set_Sys_Mes(text);
                    if (text.ToLower().StartsWith("0#"))
                    {
                        ClientMainWindow.Set_Sys_Mes(text);
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
            Encoding.ASCII.GetBytes("1#" + text);

            if (!socket.Connected)
            {
                socket.Connect(serverIPLocalv4, Vars.serverPort);
            }

            socket.Send(buffer.ToArray());
        }
    }
}
