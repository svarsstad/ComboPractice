using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        Task BacklineTask;
        static CancellationTokenSource BacklineCanselTokenSource = new CancellationTokenSource();
        static CancellationToken BacklineCanselToken = BacklineCanselTokenSource.Token;
        ClientMain clientBackend = new ClientMain();
        //public string status = "Offline";

        


        public MainWindow()
        {
            InitializeComponent();
            BacklineTask = Task.Run(() => clientBackend.Run(this, BacklineCanselToken));

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
        ~MainWindow()
        {
            End();
        }

        public void End()
        {
            BacklineCanselTokenSource.Cancel();
        }

        protected virtual void OnExit(ExitEventArgs e)
        {
            End();
        }

    }
}
