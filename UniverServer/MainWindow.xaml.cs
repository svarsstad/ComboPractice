using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UniverServer
{
    public partial class MainWindow : Window
    {
        public ServerMain serverMainInstance = new ServerMain();

        public MainWindow()
        {
            InitializeComponent();

            Cli_Sen.IsEnabled = false;
            Cli_Del.IsEnabled = false;
            Cli_Cle.IsEnabled = false;
            His_Cle.IsEnabled = false;
            His_Del.IsEnabled = false;

            Status_Box.FontSize = 14;
            Ser_Log.FontSize = 10;

            Status_Box.Text =
                "IPv4:\t\t\t" + serverMainInstance.serverIPLocalv4 + '\n' +
                "hostName:\t\t" + serverMainInstance.hostName + '\n' +
                "Status:\t\t\t" + serverMainInstance.status;

            Task.Run(() => serverMainInstance.Run(this));
        }

        public Action Refresh_Async()
        {
            Ser_Log.Dispatcher.InvokeAsync(() =>
                RefreshAll());
            return null;
        }

        public Action SetLog(string logEntry)
        {
            Ser_Log.Dispatcher.InvokeAsync(() =>
                Ser_Log.Text += logEntry + '\n');
            return null;
        }

        private void Cli_Del_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (Cli_Lis.Items.IsEmpty)
            {
                Cli_Cle.IsEnabled = false;
            }

            Monitor.Enter(ServerMain.monitorLock);

            try {
                ServerMain.Clients.RemoveAt(Cli_Lis.SelectedIndex);
            } catch (Exception e) {
                Console.WriteLine(e);
            }

            Monitor.Exit(ServerMain.monitorLock);

            Cli_Lis.Items.RemoveAt(Cli_Lis.SelectedIndex);
            Cli_Del.IsEnabled = false;
        }

        private void Cli_Cle_Click(object sender, RoutedEventArgs e)
        {
            Monitor.Enter(ServerMain.monitorLock);

            ServerMain.Clients = new List<ClientData>();

            Monitor.Exit(ServerMain.monitorLock);

            Cli_Lis.Items.Clear();
            Cli_Cle.IsEnabled = false;
        }

        private void His_Del_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (His_Lis.Items.IsEmpty)
            {
                His_Cle.IsEnabled = false;
            }

            Monitor.Enter(ServerMain.monitorLock);

            try {
                ServerMain.ClientHistory.RemoveAt(His_Lis.SelectedIndex);
            } catch (Exception e) {
                Console.WriteLine(e);
            }

            Monitor.Exit(ServerMain.monitorLock);

            His_Lis.Items.RemoveAt(His_Lis.SelectedIndex);
            His_Del.IsEnabled = false;

            if (His_Lis.Items.IsEmpty)
            {
                His_Cle.IsEnabled = false;
            }
        }

        private void His_Cle_Click(object sender, RoutedEventArgs e)
        {
            Monitor.Enter(ServerMain.monitorLock);

            ServerMain.ClientHistory = new List<ClientData>();

            Monitor.Exit(ServerMain.monitorLock);

            His_Lis.Items.Clear();
            His_Cle.IsEnabled = false;
            His_Del.IsEnabled = false;
        }

        private void Cli_Sen_Click(object sender, RoutedEventArgs e) {
            serverMainInstance.SendText(Cli_Mes.Text, Cli_Lis.SelectedIndex);
        }

        private void Bro_Sen_Click(object sender, RoutedEventArgs e) {
            //Task.Run(() => serverMainInstance.SendTextAll(Bro_Mes.Text));
            serverMainInstance.SendTextAll(Bro_Mes.Text);
        }

        private void Ref_All_Click(object sender, RoutedEventArgs e) => RefreshAll();

        private void RefreshAll()
        {
            Cli_Lis.Items.Clear();
            His_Lis.Items.Clear();

            Monitor.Enter(ServerMain.monitorLock);

            foreach (ClientData cli in ServerMain.Clients)
            {
                Cli_Lis.Items.Add(new ListBoxItem());
                (Cli_Lis.Items[Cli_Lis.Items.Count - 1] as ListBoxItem).Content = cli.id;
            }

            foreach (ClientData his in ServerMain.ClientHistory)
            {
                His_Lis.Items.Add(new ListBoxItem());
                (His_Lis.Items[His_Lis.Items.Count - 1] as ListBoxItem).Content = his.id;
            }

            Monitor.Exit(ServerMain.monitorLock);

            if (Cli_Lis.Items.IsEmpty) {
                Cli_Cle.IsEnabled = false;
            }
            if (His_Lis.Items.IsEmpty) {
                His_Cle.IsEnabled = false;
            }

            Status_Box.Text =
                "IPv4:\t\t\t" + serverMainInstance.serverIPLocalv4 + '\n' +
                "hostName:\t\t" + serverMainInstance.hostName + '\n' +
                "Status:\t\t\t" + serverMainInstance.status;
        }

        private void Cli_Lis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cli_Sen.IsEnabled = false;
            Cli_Del.IsEnabled = false;

            foreach (ListBoxItem item in Cli_Lis.Items)
            {
                if (item.IsSelected)
                {
                    Cli_Sen.IsEnabled = true;
                    Cli_Del.IsEnabled = true;
                }
            }
        }

        private void His_Lis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            His_Del.IsEnabled = false;

            foreach (ListBoxItem item in His_Lis.Items)
            {
                if (item.IsSelected)
                {
                    His_Del.IsEnabled = true;
                }
            }
        }

        protected virtual void OnExit(ExitEventArgs e)
        {
            serverMainInstance.End();
        }
    }
}
