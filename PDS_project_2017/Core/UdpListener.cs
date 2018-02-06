using Newtonsoft.Json;
using PDS_project_2017.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PDS_project_2017.Core
{
    /* This class listen to users requesting available users.
     * It answers only if the current status is "available"
     */
    class UdpListener
    {
        private UdpClient udpServer;
        private User me;
        public static ManualResetEvent statusAvailableEvent;

        public UdpListener()
        {
            udpServer = new UdpClient(55555);
            statusAvailableEvent = new ManualResetEvent(!Properties.Settings.Default.PrivateMode);

            // initing current machine identity
            me = new User
            {
                // TODO : set Name and Image as from local settings
                Name = "Andrea",
                //Image = Image.FromFile(@"C:\Users\mastinux\Pictures\mastino.jpg")
                //Image = BitmapImage2Bitmap(UserSettings.LoadImage())
                Image = UserSettings.LoadImage()
            };
        }

        public void listen() {
            while (statusAvailableEvent.WaitOne())
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
