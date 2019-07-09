﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace UniverServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public struct Client
    {
        public String IPv4;
        public String IPv6;
        public String Status;

      public Client(String pIPv4,String pIPv6,String pStatus)
        {
            IPv4 = pIPv4;
            IPv6 = pIPv6;
            Status = pStatus;
        }
    }


    public partial class MainWindow : Window
    {
        
        //List<Client> Clients = new List<Client>();
        //List<Client> ClientHistory = new List<Client>();
        ServerMain serverMainInstance = new ServerMain();

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
                "IPv6:\t\t\t" + serverMainInstance.serverIPLocalv6 + '\n' +
                "hostName:\t\t" + serverMainInstance.hostName + '\n' +
                "Status:\t\t\t" + serverMainInstance.status;



            Task.Run(() => serverMainInstance.Run(this));





        }

         public Action Refresh_Async()
        {
            Ser_Log.Dispatcher.InvokeAsync(() =>
                this.RefreshAll());
            return null;
        }

        public Action SetLog(String logEntry)
        {

            Ser_Log.Dispatcher.InvokeAsync(() =>
                this.Ser_Log.Text += logEntry + '\n');
            return null;
        }
        /*public Action AddClient(String cIPv4, String cIPv6)
        {

            Ser_Log.Dispatcher.InvokeAsync(() =>
                this.Clients.Add(new Client(cIPv4,cIPv6,"Online")));
            return null;
        }*/


        private void Cli_Del_Click(object sender, RoutedEventArgs e)
        {
            if (Cli_Lis.Items.IsEmpty)
            {
                Cli_Cle.IsEnabled = false;
            }
            Monitor.Enter(ServerMain.monitorLock);
            try { ServerMain.Clients.RemoveAt(Cli_Lis.SelectedIndex); } catch { }
            Monitor.Exit(ServerMain.monitorLock);
            Cli_Lis.Items.RemoveAt(Cli_Lis.SelectedIndex);
            Cli_Del.IsEnabled = false;
        }

        private void Cli_Cle_Click(object sender, RoutedEventArgs e)
        {
            Monitor.Enter(ServerMain.monitorLock);
            //serverMainInstance.Clients = new List<ClientData>();
            ServerMain.Clients = new List<ClientData>();
            Monitor.Exit(ServerMain.monitorLock);
            Cli_Lis.Items.Clear();
            Cli_Cle.IsEnabled = false;
        }

        private void His_Del_Click(object sender, RoutedEventArgs e)
        {

            if (His_Lis.Items.IsEmpty)
            {
                His_Cle.IsEnabled = false;
            }
            Monitor.Enter(ServerMain.monitorLock);
            try { ServerMain.ClientHistory.RemoveAt(His_Lis.SelectedIndex); } catch { }
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

        private void Cli_Sen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Bro_Sen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Ref_All_Click(object sender, RoutedEventArgs e)
        {
            RefreshAll();
            
           
        }
        public void RefreshAll()
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
            if (Cli_Lis.Items.IsEmpty) { Cli_Cle.IsEnabled = false; }
            if (His_Lis.Items.IsEmpty) { His_Cle.IsEnabled = false; }

            Status_Box.Text =
                "IPv4:\t\t\t" + serverMainInstance.serverIPLocalv4 + '\n' +
                "IPv6:\t\t\t" + serverMainInstance.serverIPLocalv6 + '\n' +
                "hostName:\t\t" + serverMainInstance.hostName + '\n' +
                "Status:\t\t\t" + serverMainInstance.status;

        }

        private void Cli_Lis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cli_Sen.IsEnabled = false;
            Cli_Del.IsEnabled = false;

            foreach(ListBoxItem  item in Cli_Lis.Items)
            {
                if (item.IsSelected) {
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

        protected virtual void OnExit(System.Windows.ExitEventArgs e)
        {
            serverMainInstance.End();

        }
    }
}
