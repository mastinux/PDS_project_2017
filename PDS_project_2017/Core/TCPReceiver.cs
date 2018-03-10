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

        // https://msdn.microsoft.com/it-it/library/fx6588te(v=vs.110).aspx

        private TcpListener _tcpServer;

        private bool _continueTransferProcess;

        public delegate void UpdateProgressBarValue(int value);
        public event UpdateProgressBarValue UpdateProgressBarEvent;

        public delegate void UpdateRemainingTimeValue(TimeSpan timeSpan);
        public event UpdateRemainingTimeValue UpdateRemainingTimeEvent;

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
        
        private void PopulateDirectory(TcpClient tcpClient, DirectoryNode directoryNode, string destinationDir)
        {
            string directoryPath = destinationDir + "\\" + directoryNode.DirectoryName;

            directoryPath = FilesUtils.CheckDirectoryExistance(directoryPath);

            Directory.CreateDirectory(directoryPath);

            foreach (var fileNode in directoryNode.FileNodes)
            {
                // TODO manage timeout and directories deletion
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
            _continueTransferProcess = true;

            // TRANSFER PROGRESS
            //TransferProgress transferProgressWindow = WindowUtils.InitTransferProgressWindow(fileNode, 0, this);
            //transferProgressWindow.CancelTransferProcessEvent = () => _continueTransferProcess = false;

            string filePath = destinationDir + "\\" + fileNode.Name;

            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = Constants.TRANSFER_TCP_READ_TIMEOUT * 1000;
            
            long fileContentLenghtReceived = 0;

            Byte[] buffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            FileStream file = FilesUtils.CreateUniqueFile(filePath);

            BinaryWriter fileWriter = new BinaryWriter(file);

            DateTime baseDateTime = DateTime.Now;

            // FILE CONTENT
            while (fileContentLenghtReceived < fileNode.Dimension && _continueTransferProcess == true)
            {
                int bytesRead = 0;

                if ((fileNode.Dimension - fileContentLenghtReceived) < buffer.Length)
                    buffer = new Byte[fileNode.Dimension - fileContentLenghtReceived];

                try
                {
                    bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException ioe)
                {
                    // sender closed connection
                    fileWriter.Close();
                    File.Delete(file.Name);

                    //WindowUtils.CloseTransferProgressWindow(transferProgressWindow);

                    return;
                }

                fileWriter.Write(buffer, 0, bytesRead);
                fileContentLenghtReceived += bytesRead;

                Thread.Sleep(Constants.TRANSFER_TCP_RECEIVER_DELAY);

                //UpdateProgressBarEvent((int)(((float)fileContentLenghtReceived / (float)fileNode.Dimension) * 100));

                TimeSpan remainingTimeSpan = TcpUtils.ComputeRemainingTime(baseDateTime, bytesRead, fileContentLenghtReceived, fileNode.Dimension);
                //UpdateRemainingTimeEvent(remainingTimeSpan);
                baseDateTime = DateTime.Now;
            }

            fileWriter.Close();

            //WindowUtils.CloseTransferProgressWindow(transferProgressWindow);
        }
    }
}
