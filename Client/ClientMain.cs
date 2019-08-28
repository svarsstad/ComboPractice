using SharedVars;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class ClientMain : IDisposable
    {
        private MainWindow clientMainWindow;
        public IPAddress serverIPLocalv4;
        public string hostName;
        private int sessionId = -1;
        private bool connected = false;
        private IPHostEntry hostEntry;
        private byte[] Databuffer = new byte[Vars.BUFFER_SIZE];
        private CancellationToken backlineCanselToken;
        private Socket socket;// = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Run(MainWindow CMW, CancellationToken pBacklineCanselToken)
        {
            clientMainWindow = CMW;
            hostName = Dns.GetHostName();
            hostEntry = Dns.GetHostEntry(hostName);
            serverIPLocalv4 = hostEntry.AddressList.Last().MapToIPv4();
            backlineCanselToken = pBacklineCanselToken;

            while (!connected && !backlineCanselToken.IsCancellationRequested)

            {
                Thread.Sleep(1);
            }
            clientMainWindow.Set_Sys_Mes("Connected");
            while (!backlineCanselToken.IsCancellationRequested)
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
            clientMainWindow.Dispatcher.InvokeAsync(clientMainWindow.EndAction());
        }

        public void Connect()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(serverIPLocalv4, Vars.SERVER_PORT, ConnectionAccepted, null);
            }
            catch (SocketException)
            {
                clientMainWindow.Set_Sys_Mes("Socket exception");  // you can put a counter here to see how many attempts are made
            }
        }

        private void ConnectionAccepted(IAsyncResult callback)
        {

            if (socket.Connected)
            {
                connected = true;
                clientMainWindow.Set_Sys_Mes("Connection Success.");
                socket.EndConnect(callback); // prevents blocking by begin connect

            }
            else
            {
                connected = false;
                clientMainWindow.Set_Sys_Mes("Connection failed.");
                socket.Close(); // prevents blocking by begin connect

            }

                return;
            
        }

        private void RecieveCallback(IAsyncResult callback)
        {
            try
            {
                int recievedSize = socket.EndReceive(callback);

                Span<byte> buffer = stackalloc byte[recievedSize];
                buffer = Databuffer;
                if (recievedSize > 0)
                {
                    string text = Encoding.ASCII.GetString(buffer.ToArray(), Vars.CLIENT_SIGN.Length, recievedSize - Vars.CLIENT_SIGN.Length);
                    if (text[0] == Vars.SERVER_SIGN[0])
                    {
                        clientMainWindow.Set_Sys_Mes(text);
                    }
                    else if(text[0] == Vars.LOGIN_REPLY_CHAR)
                    {
                        ProcessLoginReply(text.Remove(0, 1));
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

        public void Login(string username, string password)
        {
            Send(Vars.LOGIN_CHAR + username + Vars.SP_SEPARATION_CHAR + password);
        }

        public void Send(string text)
        {
            Task.Run(() =>
            {
                if (!MainWindow.BacklineCanselTokenSource.IsCancellationRequested)
                {
                    Span<byte> dataMessage = stackalloc byte[text.Length * 2];
                    dataMessage = Encoding.ASCII.GetBytes(Vars.CLIENT_SIGN + text);

                    try
                    {
                        socket.Send(dataMessage.ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
             );
        }
        private void ProcessLoginReply(string text)
        {
            sessionId = Convert.ToInt32(text);
            if (sessionId != -1)
            {
                clientMainWindow.Set_Sys_Mes("Login Successful");
            }
            else
            {
                clientMainWindow.Set_Sys_Mes("Login failed");
            }
        }

        public void Dispose()
        {
            if (socket.Connected)
            {
                Span<byte> buffer = stackalloc byte[sizeof(char)];
                buffer = Encoding.ASCII.GetBytes("~");
                socket.Send(buffer.ToArray());
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            clientMainWindow.EndAction();
        }
    }
}