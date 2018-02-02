using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    class UdpListener
    {
        private UdpClient udpServer;
        private User me;

        public UdpListener()
        {
            udpServer = new UdpClient(55555);

            // initing current machine identity
            me = new User
            {
                // TODO : set Name and Image as from local settings
                Name = "Andrea",
                Image = Image.FromFile(@"C:\Users\mastinux\Pictures\mastino.jpg")
            };
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
                    byte[] recBytes = udpServer.Receive(ref remoteIpEndPoint);

                    if ( !UdpUtils.isSelfUdpMessage(remoteIpEndPoint) )
                    {
                        // reading requester user
                        string readData = Encoding.ASCII.GetString(recBytes);

                        User requesterUser = JsonConvert.DeserializeObject<User>(readData);
                        requesterUser.Id = remoteIpEndPoint.Address.ToString();

                        Console.WriteLine(this.GetType().Name + ": received message from " + requesterUser.Name);

                        // preparing response
                        byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(me));

                        // sending response
                        udpServer.Send(byteToSend, byteToSend.Length, remoteIpEndPoint);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
