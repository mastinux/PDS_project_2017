using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            String fileName = ReceiveFileName(client);
            String destinationDir = null;

            if (Properties.Settings.Default.AutoAccept)
            {
                SendAcceptanceResponse(client);

                // retrieving default dir
                destinationDir = Properties.Settings.Default.DefaultDir;
            }
            else
            {
                // asking user for file acceptance
                FilesAcceptance fileAcceptanceWindow = new FilesAcceptance(fileName);
                // blocking call until window is closed
                fileAcceptanceWindow.ShowDialog();

                destinationDir = fileAcceptanceWindow.DestinationDir;

                if (destinationDir != null)
                {
                    // file accepted
                    SendAcceptanceResponse(client);
                }
                else
                {
                    // file not accepted or window closed
                    SendRefuseResponse(client);

                    return;
                }
            }
            
            String filePath = destinationDir + "\\" + fileName;

            // receive file content
            ReceiveFileContent(client.GetStream(), filePath);
        }

        private void SendRefuseResponse(TcpClient client)
        {
            SendResponse(client, Constants.TRANSFER_TCP_REFUSE);
        }

        private void SendAcceptanceResponse(TcpClient client)
        {
            SendResponse(client, Constants.TRANSFER_TCP_ACCEPT);
        }

        private void SendResponse(TcpClient client, String command)
        {
            Byte[] data = new Byte[Constants.TRANSFER_TCP_COMMAND_LEN];
            data = Encoding.UTF8.GetBytes(command);

            client.GetStream().Write(data, 0, data.Length);
        }

        private string ReceiveFileName(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();

            Byte[] data = new Byte[Constants.TRANSFER_TCP_FILE_NAME_LEN];

            // FILE NAME LENGHT
            networkStream.Read(data, 0, Constants.TRANSFER_TCP_FILE_NAME_LEN);
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
            Byte[] data = new Byte[Constants.TRANSFER_TCP_FILE_CONTENT_LEN];
            networkStream.Read(data, 0, Constants.TRANSFER_TCP_FILE_CONTENT_LEN);
            long fileContentLenght = BitConverter.ToInt64(data, 0);

            long fileContentLenghtReceived = 0;

            Byte[] buffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            BinaryWriter fileWriter = new BinaryWriter(File.Open(filePath, FileMode.Create));

            // FILE CONTENT
            while (fileContentLenghtReceived < fileContentLenght)
            {
                var bytesRead = networkStream.Read(buffer, 0, buffer.Length);

                fileWriter.Write(buffer, 0, bytesRead);

                fileContentLenghtReceived += bytesRead;
            }

            fileWriter.Close();
        }
    }
}
