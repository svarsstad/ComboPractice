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
            if (string.IsNullOrEmpty(pIPv4))
            {
                throw new System.ArgumentNullException("pIPv4 null", nameof(pIPv4));
            }

            if (string.IsNullOrEmpty(pIPv6))
            {
                throw new System.ArgumentNullException("pIPv6 null", nameof(pIPv6));
            }

            if (string.IsNullOrEmpty(pStatus))
            {
                throw new System.ArgumentNullException("pStatus null", nameof(pStatus));
            }

            IPv4 = pIPv4;
            IPv6 = pIPv6;
            Status = pStatus;
        }
    }
}
