using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using PDS_project_2017.Core.Entities;
using PDS_project_2017.Core.Utils;
using PDS_project_2017.UI.Utils;

namespace PDS_project_2017.Core
{
    public class TcpReceiver
    {
        // server class

        private TcpListener _tcpServer;

        public delegate void AddNewTransfer(FileTransfer transfer);
        public static event AddNewTransfer NewTransferEvent;

        public TcpReceiver(int port)
        {
            IPAddress localAddr = IPAddress.Parse(IpUtils.GetLocalIPAddress().ToString());

            _tcpServer = new TcpListener(localAddr, port);
            _tcpServer.Start();
        }

        public void Receive()
        {
            while (true)
            {
                // blocking call
                TcpClient client = _tcpServer.AcceptTcpClient();

                Thread clientThread = new Thread(() =>
                {
                    String command = TcpUtils.ReceiveCommand(client, Constants.TRANSFER_TCP_FILE.Length);

                    if (command.Equals(Constants.TRANSFER_TCP_FILE))
                    {
                        ReceiveFile(client);
                    }
                    else
                    {
                        ReceiveDirectory(client);
                    }

                    client.Close();
                });

                clientThread.IsBackground = true;
                clientThread.SetApartmentState(ApartmentState.STA);
                clientThread.Start();
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
        
        private bool PopulateDirectory(TcpClient tcpClient, DirectoryNode directoryNode, string destinationDir)
        {
            string directoryPath = destinationDir + "\\" + directoryNode.DirectoryName;
            string senderUserName = directoryNode.SenderUserName;

            directoryPath = FilesUtils.CreateUniqueDirectory(directoryPath);

            foreach (var fileNode in directoryNode.FileNodes)
            {
                fileNode.SenderUserName = senderUserName;
                bool completed = ReceiveDirectoryFile(tcpClient, fileNode, directoryPath);

                if (!completed)
                    return false;
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
            {
                innerDirectoryNode.SenderUserName = senderUserName;
                bool completed = PopulateDirectory(tcpClient, innerDirectoryNode, directoryPath);

                if (!completed)
                    return false;
            }

            return true;
        }

        private bool ReceiveDirectoryFile(TcpClient tcpClient, FileNode fileNode, string destinationDir)
        {
            TcpUtils.ReceiveCommand(tcpClient, Constants.TRANSFER_TCP_FILE.Length);

            String jsonFileNodeDescription = TcpUtils.ReceiveDescription(tcpClient);

            TcpUtils.SendAcceptanceResponse(tcpClient);

            return ReceiveFileContent(tcpClient, fileNode, destinationDir);
        }

        private bool IsConnected(TcpClient tcpclient)
        {
            if (!(tcpclient.Client.Poll(1, SelectMode.SelectRead) && tcpclient.Client.Available == 0))
                return true;
            else
                throw new IOException();
                
        }

        private bool ReceiveFileContent(TcpClient tcpClient, FileNode fileNode, string destinationDir)
        {
            string filePath = destinationDir + "\\" + fileNode.Name;

            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = Constants.TRANSFER_TCP_READ_TIMEOUT * 1000;
            
            long fileContentLenghtReceived = 0;
            Byte[] buffer = new Byte[Constants.TRANSFER_TCP_BUFFER];
            FileStream fileStream = FilesUtils.CreateUniqueFile(filePath);
            fileNode.Name = Path.GetFileName(fileStream.Name);
            BinaryWriter fileWriter = new BinaryWriter(fileStream);
            DateTime baseDateTime = DateTime.Now;

            FileTransfer fileTransfer = new FileTransfer()
            {
                File = fileNode,
                Progress = 0,
                ContinueFileTransfer = true,
                Status = TransferStatus.Pending,
                Sending = false,
                SavingPath = destinationDir,
                ManagementDateTime = DateTime.Now
            };

            NewTransferEvent(fileTransfer);

            // FILE CONTENT
            while (fileContentLenghtReceived < fileNode.Dimension && fileTransfer.ContinueFileTransfer == true)
            {
                int bytesRead;

                if ((fileNode.Dimension - fileContentLenghtReceived) < buffer.Length)
                    buffer = new Byte[fileNode.Dimension - fileContentLenghtReceived];

                try
                {
                    IsConnected(tcpClient);
                    bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException ioe)
                {
                    // sender closed connection
                    fileWriter.Close();

                    File.Delete(fileStream.Name);
                    fileTransfer.Status = TransferStatus.Error;

                    return false;
                }

                fileWriter.Write(buffer, 0, bytesRead);
                fileContentLenghtReceived += bytesRead;

                Thread.Sleep(Constants.TRANSFER_TCP_RECEIVER_DELAY);
                double progress = ((double)fileContentLenghtReceived / (double)fileNode.Dimension) * 100;
                fileTransfer.Progress = progress;

                TimeSpan remainingTimeSpan = TcpUtils.ComputeRemainingTime(baseDateTime, bytesRead, fileContentLenghtReceived, fileNode.Dimension);
                fileTransfer.RemainingTime = remainingTimeSpan;
                baseDateTime = DateTime.Now;
            }

            fileWriter.Close();

            if (!fileTransfer.ContinueFileTransfer)
            {
                File.Delete(fileStream.Name);
                fileTransfer.Status = TransferStatus.Canceled;
                
                return false;
            }
            else
            {
                fileTransfer.Status = TransferStatus.Completed;

                return true;
            }
        }
    }
}
