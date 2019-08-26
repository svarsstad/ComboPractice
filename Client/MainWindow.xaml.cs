using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task BacklineTask;
        public static CancellationTokenSource BacklineCanselTokenSource = new CancellationTokenSource();
        private static CancellationToken BacklineCanselToken = BacklineCanselTokenSource.Token;
        private ClientMain clientBackend = new ClientMain();
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

        public Action EndAction()
        {
            End();
            return null;
        }

        private void End()
        {
            OnExit(null);
        }

        protected virtual void OnExit(ExitEventArgs e)
        {
            BacklineCanselTokenSource.Cancel();
        }
    }
}