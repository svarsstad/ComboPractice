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

        public ClientData()
        {
            this.id = Guid.NewGuid().ToString();
        }
        public ClientData(Socket sc)
        {
            this.id = Guid.NewGuid().ToString();
            this.socket = sc;
        }
        public ClientData(Socket sc, Thread tr)
        {
            this.id = Guid.NewGuid().ToString();
            this.socket = sc;
            this.thread = tr;
        }
    }
}