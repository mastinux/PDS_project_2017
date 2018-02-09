using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    static class UdpUtils
    {
        // this function checks if remoteIpEndPoint address equals current machine IP address
        public static bool isSelfUdpMessage(IPEndPoint remoteIpEndPoint)
        {
            // managing only IPv4 addresses

            // TODO test purpose : remove on production environment
            return false;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {   
                    if (ip.Equals(remoteIpEndPoint.Address))
                        return true;
                }
            }

            return false;
        }
    }
}
