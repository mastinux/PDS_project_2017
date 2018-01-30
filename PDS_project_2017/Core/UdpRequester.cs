using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    class UdpRequester
    {
        private UdpClient udpClient;
        private IPEndPoint broadcastIp;
        private string requestMessage;

        public UdpRequester()
        {
            udpClient = new UdpClient();

            broadcastIp = new IPEndPoint(IPAddress.Broadcast, 55555);

            requestMessage = "talktome";
        }

        public void request()
        {
            byte[] byteToSend = Encoding.ASCII.GetBytes(requestMessage);

            udpClient.Send(byteToSend, byteToSend.Length, broadcastIp);
            

        }

    }
}
