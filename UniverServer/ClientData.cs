﻿using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UniverServer
{
    public class ClientData
    {
        public Socket socket;
        public Task task;
        public string id; //unique name(hex)
        private int sessionId = -1;
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

        public ClientData(int pI, Socket pSocket, Task pT, CancellationToken pCancellationToken)
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
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(true);
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