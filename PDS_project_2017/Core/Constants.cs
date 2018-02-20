using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
    }
}
