using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace PDS_project_2017.Core
{
    class TcpReceiver
    {
        // server class

        // https://msdn.microsoft.com/it-it/library/fx6588te(v=vs.110).aspx

        private TcpListener _tcpServer;

        public TcpReceiver()
        {
            IPAddress localAddr = IPAddress.Parse(IpUtils.GetLocalIPAddress().ToString());

            _tcpServer = new TcpListener(localAddr, Constants.TRANSFER_TCP_PORT);

            _tcpServer.Start();
        }

        public void Receive()
        {
            while (true)
            {
                // blocking call
                TcpClient client = _tcpServer.AcceptTcpClient();

                ReceiveFile(client);
            }
        }

        private void ReceiveFile(TcpClient client)
        {
            // receive file name
            String fileName = ReceiveFileName(client.GetStream());

            String destinationDir = null;

            if (Properties.Settings.Default.AutoAccept)
            {
                Console.WriteLine("auto accepting file");

                // retrieving default dir
                destinationDir = Properties.Settings.Default.DefaultDir;
            }
            else
            {
                Console.WriteLine("asking user for acceptance and destination dir");

                // asking user if want the file
                FilesAcceptance fileAcceptanceWindow = new FilesAcceptance(fileName);
                // blocking call until window is closed
                fileAcceptanceWindow.ShowDialog();

                destinationDir = fileAcceptanceWindow.DestinationDir;

                if (destinationDir == null)
                    // file not accepted or window closed
                    return;
            }
            
            String filePath = destinationDir + "\\" + fileName;

            Console.WriteLine("saving file " + filePath);

            // receive file content
            ReceiveFileContent(client.GetStream(), filePath);
        }

        private string ReceiveFileName(NetworkStream networkStream)
        {

            Byte[] data = new Byte[1];

            // FILE NAME LENGHT
            networkStream.Read(data, 0, 1);
            int fileNameDataLenght = Convert.ToInt32(data[0]);

            data = new byte[fileNameDataLenght];

            // FILE NAME
            networkStream.Read(data, 0, fileNameDataLenght);
            String fileName = System.Text.Encoding.UTF8.GetString(data);

            return fileName;
        }

        private void ReceiveFileContent(NetworkStream networkStream, String filePath)
        {
            // FILE CONTENT LENGHT
            Byte[] data = new Byte[8];
            networkStream.Read(data, 0, 8);
            long fileContentLenght = BitConverter.ToInt64(data, 0);

            long fileContentLenghtReceived = 0;

            Byte[] buffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            //Console.WriteLine("saving file in " + filePath);

            BinaryWriter fileWriter = new BinaryWriter(File.Open(filePath, FileMode.Create));

            // FILE CONTENT
            while (fileContentLenghtReceived < fileContentLenght)
            {
                fileContentLenghtReceived += networkStream.Read(buffer, 0, buffer.Length);

                fileWriter.Write(buffer, 0, buffer.Length);
            }

            //Console.WriteLine("file received");
        }
    }
}
