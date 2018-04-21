namespace PDS_project_2017.Core
{
    static class Constants
    {
        // upd discovery
        public const int AVAILABLE_USERS_UPDATE_INTERVAL = 5;
        public const int DISCOVERY_UDP_PORT = 55555;

        // tcp file transfer
        public const int TRANSFER_TCP_PORT = 55556;
        public const int TRANSFER_TCP_BUFFER = 1024;
        public const int TRANSFER_TCP_FILE_CONTENT_LEN = 8;
        public const int TRANSFER_TCP_INT_LEN = 4;
        public const int TRANSFER_TCP_COMMAND_LEN = 3;

        public const string TRANSFER_TCP_FILE = "FIL";
        public const string TRANSFER_TCP_DIRECTORY = "DIR";
        public const string TRANSFER_TCP_ACCEPT = "OK ";
        public const string TRANSFER_TCP_REFUSE = "NO ";

        public static int TRANSFER_TCP_READ_TIMEOUT = 5;
        public static int TRANSFER_TCP_WRITE_TIMEOUT = 5;

        // windows positions
        private static double offset = 70;
        public static double SENDER_WINDOW_TOP = offset;
        public static double SENDER_WINDOW_LEFT = offset;
        public static double RECEIVER_WINDOW_TOP = offset;
        public static double RECEIVER_WINDOW_LEFT = System.Windows.SystemParameters.WorkArea.Width / 2 + offset;

        // delays
        public static int TRANSFER_TCP_SENDER_DELAY = 100;
        public static int TRANSFER_TCP_RECEIVER_DELAY = TRANSFER_TCP_SENDER_DELAY * 0;
        public static int BALLOONTIP_DELAY = 1000;

        // IPs
        public static string BROADCAST_IP = "255.255.255.255";
        public static string HAMACHI_BROADCAST_IP = "25.255.255.255";

        // test 
        public static bool FAKE_USERS = true;
    }
}
