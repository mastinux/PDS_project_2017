using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using PDS_project_2017.UI;

namespace PDS_project_2017.Core
{
    class UdpRequester
    {
        // https://msdn.microsoft.com/it-it/library/ts553s52(v=vs.110).aspx
        
        private UdpClient udpClient;
        private IPEndPoint broadcastIp;
        private User me;

        public delegate void addAvailableUser(User newUser);
        public event addAvailableUser addUserEvent;

        public delegate void cleanAvailableUser();
        public event cleanAvailableUser cleanUsersEvent;

        private volatile bool keepRequestingStatus;
        
        public UdpRequester()
        {
            keepRequestingStatus = true;

            udpClient = new UdpClient();
            udpClient.Client.ReceiveTimeout = Constants.AVAILABLE_USERS_UPDATE_INTERVAL * 1000;

            broadcastIp = new IPEndPoint(IPAddress.Broadcast, Constants.UDP_PORT);
            
            // initing current user identity
            me = new User
            {
                Name = UserSettings.LoadName()
            };
        }

        public void retrieveAvailableUsers()
        {
            // preparing request
            byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(me));

            while (keepRequestingStatus) 
            {
                bool timedOut = false;

                // sending request
                udpClient.Send(byteToSend, byteToSend.Length, broadcastIp);

                // ip end point used to record address and port of sender
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (!timedOut)
                {
                    byte[] recBytes = null;

                    try
                    {
                        // blocked until a message is received
                        recBytes = udpClient.Receive(ref remoteIpEndPoint);
                    }
                    catch (SocketException e)
                    {
                        // TODO re launch exception if not equal to TimedOut
                        
                        if ( !e.SocketErrorCode.Equals(SocketError.TimedOut) )
                            throw  new Exception("Unexpected exception", e);

                        timedOut = true;

                        // managing timeout
                        cleanUsersEvent();
                    }

                    if (!UdpUtils.isSelfUdpMessage(remoteIpEndPoint) && !timedOut)
                    {
                        // reading response
                        string readData = Encoding.ASCII.GetString(recBytes);

                        // parsing available user
                        User availableUser = JsonConvert.DeserializeObject<User>(readData);
                        availableUser.Id = remoteIpEndPoint.Address.ToString();
                        availableUser.LastUpTime = DateTime.Now;

                        // call the functions registered to the delegate, in particular in userSelection
                        addUserEvent(availableUser);
                    }
                }
            }
        }

        public void StopRequests()
        {
            keepRequestingStatus = false;
        }
    }
}
