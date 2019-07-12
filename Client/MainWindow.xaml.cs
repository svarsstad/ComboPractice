using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;


namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        ClientMain clientBackend = new ClientMain();
        //public string status = "Offline";

        


        public MainWindow()
        {
            InitializeComponent();

            Task.Run(() => clientBackend.Run(this));

        }

        private void Cli_Sen_Click(object sender, RoutedEventArgs eventArgs)
        {
            clientBackend.Send(Cli_Mes.Text);

        }

        public Action Set_Sys_Mes(string text)
        {
            sys_mes.Dispatcher.InvokeAsync(() =>
                sys_mes.Text = text);
            return null;
        }
        protected virtual void OnExit(ExitEventArgs e)
        {
            clientBackend.End();
        }

    }
}
