using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PDS_project_2017.UI;

namespace PDS_project_2017.Core
{
    class UdpRequester
    {
        // https://msdn.microsoft.com/it-it/library/ts553s52(v=vs.110).aspx
        
        private UdpClient _udpClient;
        private IPEndPoint _broadcastIp;
        private User _me;
        private volatile bool _continueRequesting;

        public delegate void AddAvailableUser(User newUser);
        public event AddAvailableUser AddUserEvent;

        public delegate void CleanAvailableUser();
        public event CleanAvailableUser CleanUsersEvent;
        
        public UdpRequester()
        {
            _continueRequesting = true;

            _udpClient = new UdpClient();
            _udpClient.Client.ReceiveTimeout = Constants.AVAILABLE_USERS_UPDATE_INTERVAL * 1000;

            _broadcastIp = new IPEndPoint(IPAddress.Parse("25.255.255.255"), Constants.DISCOVERY_UDP_PORT);

            // initing current user identity
            _me = new User
            {
                Name = UserSettings.LoadName()
            };
        }

        public void RetrieveAvailableUsers()
        {
            // preparing request
            byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_me));

            while (_continueRequesting) 
            {
                bool timedOut = false;

                // sending request
                _udpClient.Send(byteToSend, byteToSend.Length, _broadcastIp);

                // ip end point used to record address and port of sender
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (!timedOut)
                {
                    byte[] recBytes = null;

                    try
                    {
                        // blocked until a message is received
                        recBytes = _udpClient.Receive(ref remoteIpEndPoint);
                    }
                    catch (SocketException e)
                    {
                        if ( !e.SocketErrorCode.Equals(SocketError.TimedOut) )
                            throw  new Exception("Unexpected exception", e);

                        timedOut = true;

                        // managing timeout
                        CleanUsersEvent();
                    }

                    if (!IpUtils.isSelfMessage(remoteIpEndPoint) && !timedOut)
                    {
                        // reading response
                        string readData = Encoding.ASCII.GetString(recBytes);

                        // parsing available user
                        User availableUser = JsonConvert.DeserializeObject<User>(readData);
                        availableUser.Id = remoteIpEndPoint.Address.ToString();
                        availableUser.LastUpTime = DateTime.Now;

                        // call the functions registered to the delegate, in particular in userSelection
                        AddUserEvent(availableUser);
                    }
                }
            }
        }

        public void StopRequesting()
        {
            _continueRequesting = false;
        }
    }
}
