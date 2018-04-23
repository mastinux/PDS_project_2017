using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PDS_project_2017.Core.Utils
{
    static class IpUtils
    {
        // this function checks if remoteIpEndPoint address equals current machine IP address
        public static bool IsSelfMessage(IPEndPoint remoteIpEndPoint)
        {
            // managing only IPv4 addresses
            if (GetLocalHamachiIpAddress().Equals(remoteIpEndPoint.Address))
                return true;
            else
                return false;
        }

        public static IPAddress GetLocalIpAddress()
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

        public static IPAddress GetLocalHamachiIpAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (ni.Name.Equals("Hamachi"))
                    {

                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address;
                            }
                        }
                    }
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
