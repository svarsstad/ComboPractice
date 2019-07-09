using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int serverPort = 8083;
        Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
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

        private void Cli_Sen_Click(object sender, RoutedEventArgs e)
        {
            string text = Cli_Mes.Text;
            byte[] buffer = Encoding.ASCII.GetBytes("1#"+text);

            if (!socket.Connected)
            {
                sys_mes.Text = "Socket exception";
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
