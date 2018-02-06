using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    class UdpRequester
    {
        // https://msdn.microsoft.com/it-it/library/ts553s52(v=vs.110).aspx

        private UdpClient udpClient;
        private IPEndPoint broadcastIp;
        private User me;

        public delegate void addAvailableUser(User newUser);
        public event addAvailableUser userEvent;


        public UdpRequester()
        {
            udpClient = new UdpClient();

            broadcastIp = new IPEndPoint(IPAddress.Broadcast, 55555);
            
            // initing current user identity
            me = new User
            {
                // TODO : set Name as from local settings
                Name = "Andrea"
            };

            
        }

        public void retrieveAvailableUsers()
        {   
            // preparing request
            byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(me));

            // sending request
            udpClient.Send(byteToSend, byteToSend.Length, broadcastIp);

            Console.WriteLine(this.GetType().Name + " : retrieving available users");

            while (true)
            {
                // ip end point used to record address and port of sender
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // blocked until a message is received
                byte[] recBytes = udpClient.Receive(ref remoteIpEndPoint);

                if (!UdpUtils.isSelfUdpMessage(remoteIpEndPoint))
                {
                    // reading response
                    string readData = Encoding.ASCII.GetString(recBytes);

                    // parsing available user
                    User availableUser = JsonConvert.DeserializeObject<User>(readData);
                    availableUser.Id = remoteIpEndPoint.Address.ToString();

                    Console.WriteLine(this.GetType().Name + " : found " + availableUser.Id + " " + availableUser.Name + " " + availableUser.Image);

                    // call the functions registered to the delegate, in particular in userSelection
                    userEvent(availableUser);
                }
            }
        }
    }
}
