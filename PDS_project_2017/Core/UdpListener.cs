using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    class UdpListener
    {
        private UdpClient listenerUdpClient;

        public UdpListener()
        {
            Console.WriteLine(this.GetType().Name + ": constructor");
            // creating upd client
            listenerUdpClient = new UdpClient(55555);
        }
        
        public void listen() {
            while (true)
            {
                try
                {
                    Console.WriteLine(this.GetType().Name + ": waiting for message");

                    // ip end point used to record address and port of sender
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // blocked until a message is received
                    byte[] recBytes = listenerUdpClient.Receive(ref remoteIpEndPoint);

                    string readData = Encoding.ASCII.GetString(recBytes);

                    // TODO manage self message

                    Console.WriteLine(readData.ToString());
                    Console.WriteLine(remoteIpEndPoint.Address.ToString());
                    Console.WriteLine(remoteIpEndPoint.Port.ToString());

                    User me = new User();
                    me.Name = "mastinux";
                    me.Image = "mastinux_image";


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
