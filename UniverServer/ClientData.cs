using System.Net.Sockets;
using System.Threading.Tasks;
using System;

namespace UniverServer
{
    public class ClientData
    {
        public Socket socket;
        public Task task;
        public string id; //unique name(hex)
        public int i; //id number/ index number

        public ClientData(int idn, Socket sc)
        {
            id = Guid.NewGuid().ToString();
            socket = sc;
            task = null;
            this.i = idn;
        }
        public ClientData(int idn, Socket sc,Task t)
        {
            id = Guid.NewGuid().ToString();
            socket = sc;
            task = t;
            this.i = idn;
        }
    }
}