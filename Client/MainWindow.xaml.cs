using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Linq;
using SharedVars;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool exit = false;
        public IPAddress serverIPLocalv4;
        public string hostName;
        IPHostEntry hostEntry;

        
        public string status = "Offline";

        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public MainWindow()
        {
            InitializeComponent();
            hostName = Dns.GetHostName();
            hostEntry = Dns.GetHostEntry(hostName);
            serverIPLocalv4 = hostEntry.AddressList.Last().MapToIPv4();
            while (!socket.Connected)
            {
                try
                {
                    socket.Connect(serverIPLocalv4, Vars.serverPort);
                    
                }
                catch (SocketException)
                {
                    sys_mes.Text = "Socket exception";  // you can put a counter here to see how many attempts are made
                }
            }
            sys_mes.Text = "Connected";
            while (!exit)
            {
                Span<byte> buffer = new Span<byte>();
                socket.BeginReceive()

            }
        }

        private void Cli_Sen_Click(object sender, RoutedEventArgs eventArgs)
        {
            string text = Cli_Mes.Text;
            byte[] buffer = Encoding.ASCII.GetBytes("1#"+text);

            if (!socket.Connected)
            {
                sys_mes.Text = "Socket not connected";
                socket.Connect(IPAddress.Loopback, serverPort);
            }
            else
            {
                sys_mes.Text = "Connection good";
            }

            socket.Send(buffer);
        }
    }
}
