using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace PDS_project_2017.Core
{
    class TCPReceiver
    {
        // server class

        // https://msdn.microsoft.com/it-it/library/fx6588te(v=vs.110).aspx

        private TcpListener _tcpServer;

        public TCPReceiver()
        {
            IPAddress localAddr = IPAddress.Parse(IpUtils.GetLocalIPAddress().ToString());

            _tcpServer = new TcpListener(localAddr, Constants.TRANSFER_TCP_PORT);

            _tcpServer.Start();
        }

        public void Receive()
        {
            while (true)
            {
                TcpClient client = _tcpServer.AcceptTcpClient();

                NetworkStream networkStream = client.GetStream();

                Byte[] data = new Byte[1];

                // FILE NAME LENGHT
                networkStream.Read(data, 0, 1);
                int fileNameDataLenght = Convert.ToInt32(data[0]);

                data = new byte[fileNameDataLenght];

                // FILE NAME
                networkStream.Read(data, 0, fileNameDataLenght);
                String fileName = System.Text.Encoding.UTF8.GetString(data);

                // FILE CONTENT LENGHT
                data = new Byte[8];
                networkStream.Read(data, 0, 8);
                long fileContentLenght = BitConverter.ToInt64(data, 0);

                // FILE CONTENT
                // TODO continue developing
            }
        }
    }
}
