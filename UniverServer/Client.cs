namespace UniverServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public struct Client
    {
        public string IPv4;
        public string IPv6;
        public string Status;

        public Client(string pIPv4, string pIPv6, string pStatus)
        {
            IPv4 = pIPv4;
            IPv6 = pIPv6;
            Status = pStatus;
        }
    }
}
