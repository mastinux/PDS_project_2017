using System;
using System.Net;
using System.Net.Sockets;

namespace PDS_project_2017.Core
{
    static class IpUtils
    {
        // this function checks if remoteIpEndPoint address equals current machine IP address
        public static bool isSelfMessage(IPEndPoint remoteIpEndPoint)
        {
            // managing only IPv4 addresses

            // TODO test purpose : remove on production environment
            if (Constants.FAKE_USERS)
                return false;

            if (GetLocalIPAddress().Equals(remoteIpEndPoint.Address))
                return true;
            else
                return false;
        }

        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
