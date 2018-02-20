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
using Newtonsoft.Json;
using PDS_project_2017.Core.Utils;

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

                String command = TcpUtils.ReceiveCommand(client, Constants.TRANSFER_TCP_FILE.Length);

                if (command.Equals(Constants.TRANSFER_TCP_FILE))
                {
                    ReceiveFile(client);
                }
                else
                {
                    ReceiveDirectory(client);
                }
            }
        }

        private void ReceiveFile(TcpClient client)
        {
            // receive file name
            String fileName = TcpUtils.ReceiveDescription(client);
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

                if (fileAcceptanceWindow.AreFilesAccepted)
                {
                    // file accepted
                    SendAcceptanceResponse(client);

                    destinationDir = fileAcceptanceWindow.DestinationDir;
                }
                else
                {
                    // file not accepted or window closed
                    SendRefuseResponse(client);

                    return;
                }
            }

            string filePath = destinationDir + "\\" + fileName;

            ReceiveFileContent(client, filePath);
        }

        private void ReceiveDirectory(TcpClient client)
        {
            string directoryDescription = TcpUtils.ReceiveDescription(client);
            
            DirectoryNode directoryNode = JsonConvert.DeserializeObject<DirectoryNode>(directoryDescription);

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
                FilesAcceptance fileAcceptanceWindow = new FilesAcceptance(directoryNode);
                // TODO create viewTree in window

                // blocking call until window is closed
                fileAcceptanceWindow.ShowDialog();

                if (fileAcceptanceWindow.AreFilesAccepted)
                {
                    // file accepted
                    SendAcceptanceResponse(client);

                    destinationDir = fileAcceptanceWindow.DestinationDir;
                }
                else
                {
                    // file not accepted or window closed
                    SendRefuseResponse(client);

                    return;
                }
            }

            PopulateDirectory(client, directoryNode, destinationDir);
        }

        private void PopulateDirectory(TcpClient tcpClient, DirectoryNode directoryNode, string destinationDir)
        {
            string directoryPath = destinationDir + "\\" + directoryNode.DirectoryName;
            
            Directory.CreateDirectory(directoryPath);

            foreach (var fileName in directoryNode.FileNameNodes)
            {
                // TODO manage unordered file receivements

                string filePath = directoryPath + "\\" + fileName;
                ReceiveDirectoryFile(tcpClient, filePath);
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
            {
                PopulateDirectory(tcpClient, innerDirectoryNode, directoryPath);
            }
        }

        private void ReceiveDirectoryFile(TcpClient tcpClient, string filePath)
        {
            TcpUtils.ReceiveCommand(tcpClient, Constants.TRANSFER_TCP_FILE.Length);

            String fileName = TcpUtils.ReceiveDescription(tcpClient);

            SendAcceptanceResponse(tcpClient);

            ReceiveFileContent(tcpClient, filePath);
        }

        private void SendRefuseResponse(TcpClient client)
        {
            TcpUtils.SendCommand(client, Constants.TRANSFER_TCP_REFUSE);
        }

        private void SendAcceptanceResponse(TcpClient client)
        {
            TcpUtils.SendCommand(client, Constants.TRANSFER_TCP_ACCEPT);
        }
        
        private void ReceiveFileContent(TcpClient tcpClient, String filePath)
        {
            NetworkStream networkStream = tcpClient.GetStream();

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
