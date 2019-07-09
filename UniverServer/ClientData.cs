using System.Net.Sockets;
using System.Threading;
using System;

namespace UniverServer
{
    public class ClientData
    {
        public Socket socket;
        public Thread thread;
        public string id;

        public ClientData(Socket sc)
        {
            id = Guid.NewGuid().ToString();
            socket = sc;
        }
    }
}