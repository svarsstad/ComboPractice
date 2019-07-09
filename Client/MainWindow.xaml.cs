using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int serverPort = 8083;
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public MainWindow()
        {
            InitializeComponent();
            while (!socket.Connected)
            {
                try
                {
                    socket.Connect(IPAddress.Loopback, serverPort);
                    sys_mes.Text = "Connected";
                }
                catch (SocketException)
                {
                    sys_mes.Text = "Socket exception";  // you can put a counter here to see how many attempts are made
                }
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
