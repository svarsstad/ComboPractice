using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace UniverServer
{
    public class ClientData
    {
        public Socket socket;
        public Task task;
        public string id; //unique name(hex)
        public int i; //id number/ index number
        public bool dataToSend = false;
        public CancellationToken cancellationToken;
        public ClientData(int idn, Socket sc, CancellationToken pCancellationToken)
        {
            id = Guid.NewGuid().ToString();
            socket = sc;
            task = null;
            this.i = idn;
            cancellationToken = pCancellationToken;
        }
        public ClientData(int pI, Socket pSocket,Task pT, CancellationToken pCancellationToken)
        {
            id = Guid.NewGuid().ToString();
            socket = pSocket;
            task = pT;
            this.i = pI;
            cancellationToken = pCancellationToken;
        }
        public void End()
        {
            
            if (socket != null && socket.Connected)
            {
                try
                {
                    socket.Disconnect(false);
                }
                catch (SocketException e)
                {
                    System.Console.WriteLine(e);
                }
            }
            if (task != null)
            {
                task.Dispose();
            }
        }
        ~ClientData()
        {
            End();
        }
    }
}