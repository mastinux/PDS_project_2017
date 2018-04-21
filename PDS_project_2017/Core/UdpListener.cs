﻿using Newtonsoft.Json;
using PDS_project_2017.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

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
        private static ManualResetEvent _statusAvailableEvent;
        
        public UdpListener()
        {
            _udpServer = new UdpClient(Constants.DISCOVERY_UDP_PORT);
            _statusAvailableEvent = new ManualResetEvent(!Properties.Settings.Default.PrivateMode);
            
            // initing current machine identity
            UpdateMe();
        }

        private void UpdateMe()
        {
            _me = new User
            {
                Name = UserSettings.LoadName(),
                Image = UserSettings.LoadImage()
            };
        }

        public static void SetStatusAvailableEvent()
        {
            _statusAvailableEvent.Set();
        }

        public static void ResetStatusAvailableEvent()
        {

            _statusAvailableEvent.Reset();
        }

        public void listen() {

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
                        string readData = Encoding.ASCII.GetString(recBytes);

                        User requesterUser = JsonConvert.DeserializeObject<User>(readData);
                        requesterUser.Id = remoteIpEndPoint.Address.ToString();

                        UpdateMe();

                        // preparing response
                        byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_me));

                        // sending response
                        _udpServer.Send(byteToSend, byteToSend.Length, remoteIpEndPoint);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("problems while udp responding, " + e.ToString());
                }
            }
        }

        private void testSendUser(IPEndPoint remoteIpEndPoint, string name)
        {
            _me.Name = name;

            // preparing response
            byte[] byteToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_me));

            // sending response
            _udpServer.Send(byteToSend, byteToSend.Length, remoteIpEndPoint);
        }

        private void testSendMultipleUsers(IPEndPoint remoteIpEndPoint)
        {
            List<string> names = new List<string>();
            names.Add("Antonio");
            names.Add("Anna");
            names.Add("Vincenzo");
            names.Add("Michele");
            names.Add("Elia");
            names.Add("Michel");
            names.Add("Giuseppe");
            names.Add("Leandro");
            names.Add("Matteo");
            names.Add("Gianpiero");

            Random random = new Random();

            int totalResponses = random.Next(0, names.Count + 1);

            for (int i = 0; i < totalResponses; i++)
            {
                int pos = random.Next(0, names.Count);

                testSendUser(remoteIpEndPoint, names[pos]);

                names.Remove(names[pos]);
            }

            //testSendUser(remoteIpEndPoint, "PAUL");
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
