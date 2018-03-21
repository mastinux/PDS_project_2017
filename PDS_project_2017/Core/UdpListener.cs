using Newtonsoft.Json;
using PDS_project_2017.UI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using PDS_project_2017.UI.Utils;

namespace PDS_project_2017.Core
{
    /*
     * This class listen to users requesting available users.
     * It answers only if the current status is "available"
     */
    public class UdpListener
    {
        private UdpClient _udpServer;
        private User _me;
        // it notifies the waiting thread that an event has occurred
        private static ManualResetEvent _statusAvailableEvent;
        
        public UdpListener()
        {
            _udpServer = new UdpClient(Constants.DISCOVERY_UDP_PORT);
            _statusAvailableEvent = new ManualResetEvent(!Properties.Settings.Default.PrivateMode);
        }

        private void UpdateMe()
        {
            _me = new User
            {
                Name = InterfaceUtils.LoadName(),
                Image = InterfaceUtils.LoadImage()
            };
        }

        public static void SetStatusAvailableEvent()
        {
            // responding to udp requests
            _statusAvailableEvent.Set();
        }

        public static void ResetStatusAvailableEvent()
        {
            // not responding to udp requests
            _statusAvailableEvent.Reset();
        }

        public void Listen() {

            while (_statusAvailableEvent.WaitOne())
            {   
                try
                {
                    // ip end point used to record address and port of sender
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // blocked until a message is received
                    byte[] recBytes = _udpServer.Receive(ref remoteIpEndPoint);

                    if ( !IpUtils.isSelfMessage(remoteIpEndPoint) )
                    {
                        // reading requester user
                        //string readData = Encoding.ASCII.GetString(recBytes);

                        //User requesterUser = JsonConvert.DeserializeObject<User>(readData);
                        //requesterUser.IPAddress = remoteIpEndPoint.Address.ToString();

                        // updating machine identity
                        UpdateMe();

                        // preparing response
                        byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_me));

                        // sending response
                        _udpServer.Send(byteToSend, byteToSend.Length, remoteIpEndPoint);

                        // TODO test purpose
                        if (Constants.FAKE_USERS)
                            Test_SendMultipleUsers(remoteIpEndPoint);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("problems while udp responding");
                    Console.WriteLine(e);
                }
            }
        }

        private void Test_SendUser(IPEndPoint remoteIpEndPoint, string name)
        {
            _me.Name = name;

            // preparing response
            byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_me));

            // sending response
            _udpServer.Send(byteToSend, byteToSend.Length, remoteIpEndPoint);
        }

        private void Test_SendMultipleUsers(IPEndPoint remoteIpEndPoint)
        {
            List<string> names = new List<string>
            {
                "Antonio",
                "Anna",
                "Vincenzo",
                "Michele",
                "Elia",
                "Michel",
                "Giuseppe",
                "Leandro",
                "Matteo",
                "Gianpiero"
            };

            Random random = new Random();

            int totalResponses = random.Next(0, names.Count + 1);

            for (int i = 0; i < totalResponses; i++)
            {
                int pos = random.Next(0, names.Count);

                Test_SendUser(remoteIpEndPoint, names[pos]);

                names.Remove(names[pos]);
            }
        }
    }
}
