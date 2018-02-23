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
using PDS_project_2017.Core.Entities;
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
            String jsonFileNode = TcpUtils.ReceiveDescription(client);
            FileNode fileNode = JsonConvert.DeserializeObject<FileNode>(jsonFileNode);

            String destinationDir = null;

            // retrieveing destination dir
            if (Properties.Settings.Default.AutoAccept)
            {
                TcpUtils.SendAcceptanceResponse(client);

                // default dir
                destinationDir = Properties.Settings.Default.DefaultDir;
            }
            else
            {
                // asking user for file acceptance
                FilesAcceptance fileAcceptanceWindow = new FilesAcceptance(fileNode);
                // blocking call until window is closed
                fileAcceptanceWindow.ShowDialog();

                if (fileAcceptanceWindow.AreFilesAccepted)
                {
                    // file accepted
                    TcpUtils.SendAcceptanceResponse(client);

                    destinationDir = fileAcceptanceWindow.DestinationDir;
                }
                else
                {
                    // file not accepted or window closed
                    TcpUtils.SendRefuseResponse(client);

                    return;
                }
            }

            ReceiveFileContent(client, fileNode, destinationDir);
        }
        
        private void ReceiveDirectory(TcpClient client)
        {
            string jsonDirectoryNodeDescription = TcpUtils.ReceiveDescription(client);
            DirectoryNode directoryNode = JsonConvert.DeserializeObject<DirectoryNode>(jsonDirectoryNodeDescription);

            String destinationDir = null;

            // retrieving destination dir
            if (Properties.Settings.Default.AutoAccept)
            {
                TcpUtils.SendAcceptanceResponse(client);

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
                    TcpUtils.SendAcceptanceResponse(client);

                    destinationDir = fileAcceptanceWindow.DestinationDir;
                }
                else
                {
                    // file not accepted or window closed
                    TcpUtils.SendRefuseResponse(client);

                    return;
                }
            }

            PopulateDirectory(client, directoryNode, destinationDir);
        }
        
        private void PopulateDirectory(TcpClient tcpClient, DirectoryNode directoryNode, string destinationDir)
        {
            string directoryPath = destinationDir + "\\" + directoryNode.DirectoryName;

            directoryPath = FilesUtils.CheckDirectoryExistance(directoryPath);

            Directory.CreateDirectory(directoryPath);

            foreach (var fileNode in directoryNode.FileNodes)
            {
                ReceiveDirectoryFile(tcpClient, fileNode, directoryPath);
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
            {
                PopulateDirectory(tcpClient, innerDirectoryNode, directoryPath);
            }
        }

        private void ReceiveDirectoryFile(TcpClient tcpClient, FileNode fileNode, string destinationDir)
        {
            TcpUtils.ReceiveCommand(tcpClient, Constants.TRANSFER_TCP_FILE.Length);

            String jsonFileNodeDescription = TcpUtils.ReceiveDescription(tcpClient);

            TcpUtils.SendAcceptanceResponse(tcpClient);

            ReceiveFileContent(tcpClient, fileNode, destinationDir);
        }
        
        private void ReceiveFileContent(TcpClient tcpClient, FileNode fileNode, string destinationDir)
        {
            string filePath = destinationDir + "\\" + fileNode.Name;

            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = Constants.TRANSFER_TCP_READ_TIMEOUT * 1000;
            
            long fileContentLenghtReceived = 0;

            Byte[] buffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            filePath = FilesUtils.CheckFileExistance(filePath);

            FileStream file = File.Open(filePath, FileMode.Create);
            BinaryWriter fileWriter = new BinaryWriter(file);

            // FILE CONTENT
            while (fileContentLenghtReceived < fileNode.Dimension)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException ioe)
                {
                    // sender closed connection
                    fileWriter.Close();
                    return;
                }

                fileWriter.Write(buffer, 0, bytesRead);

                fileContentLenghtReceived += bytesRead;
            }

            fileWriter.Close();
        }
    }
}
