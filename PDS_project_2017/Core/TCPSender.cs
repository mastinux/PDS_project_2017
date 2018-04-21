using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using PDS_project_2017.Core.Entities;
using PDS_project_2017.Core.Utils;
using PDS_project_2017.UI.Utils;

namespace PDS_project_2017.Core
{
    public class TCPSender
    {
        // client class
        
        private string _path;
        private string _destinationUserName;
        private string _server;
        private int _port;

        public delegate void AddNewTransfer(FileTransfer transfer);
        public static event AddNewTransfer NewTransferEvent;

        public TCPSender(string server, int port, string destinationUserName, string path)
        {
            _server = server;
            _port = port;

            _destinationUserName = destinationUserName;
            
            _path = path;
        }

        public void SendFile()
        {
            TcpClient tcpClient = new TcpClient(_server, _port);
            SendDirectoryFile(tcpClient, _path);
            tcpClient.Close();
        }

        public void SendFile(string path)
        {
            TcpClient tcpClient = new TcpClient(_server, _port);
            SendDirectoryFile(tcpClient, path);
            tcpClient.Close();
        }

        private void SendDirectoryFile(TcpClient tcpClient, string filePath)
        {
            TcpUtils.SendFileRequest(tcpClient);

            SendFileNodeDescription(tcpClient, filePath);

            string responseCommand = TcpUtils.ReceiveCommand(tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);
            if (!responseCommand.Equals(Constants.TRANSFER_TCP_ACCEPT))
            {
                ManageRefusedFile(filePath);
                return;
            }

            SendFileContent(tcpClient, filePath);
        }

        private void ManageRefusedFile(string filePath)
        {
            // preparing file node for file transfer
            FileNode fileNode = new FileNode
            {
                Name = Path.GetFileName(filePath),
                ReceiverUserName = _destinationUserName
            };

            // preparing file transfer for main window
            FileTransfer fileTransfer = new FileTransfer()
            {
                File = fileNode,
                Status = TransferStatus.Refused
            };

            InterfaceUtils.ShowBalloonTip(fileTransfer);
        }

        private void SendFileContent(TcpClient tcpClient, string filePath)
        {
            // preparing file stream for reading
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long fileDimension = fileStream.Length;

            // preparing network stream for writing
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.WriteTimeout = Constants.TRANSFER_TCP_WRITE_TIMEOUT;

            // preparing file node for file transfer
            FileNode fileNode = new FileNode
            {
                Name = Path.GetFileName(filePath),
                SenderUserName = Properties.Settings.Default.Name,
                ReceiverUserName = _destinationUserName
            };

            // preparing file transfer for main window
            FileTransfer fileTransfer = new FileTransfer()
            {
                File = fileNode,
                Progress = 0,
                ContinueFileTransfer = true,
                Status = TransferStatus.Pending,
                Sending = true,
                ManagementDateTime = DateTime.Now
            };

            NewTransferEvent(fileTransfer);
            
            Byte[] fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            int bytesRead;
            long totalBytesRead = 0;

            // start time for progress bar value calculation
            DateTime startDateTime = DateTime.Now;

            // iterating while reading file
            while ((bytesRead = fileStream.Read(fileContentBuffer, 0, fileContentBuffer.Length)) > 0 && fileTransfer.ContinueFileTransfer)
            {
                try
                {
                    // writing file content on network stream
                    networkStream.Write(fileContentBuffer, 0, bytesRead);
                }
                catch (IOException ioe)
                {
                    fileTransfer.Status = TransferStatus.Error;
                    return;
                }
                
                totalBytesRead += bytesRead;
                double progress = (((double)totalBytesRead / (double)fileStream.Length) * 100);
                fileTransfer.Progress = progress;

                //TODO test purpose
                Thread.Sleep(Constants.TRANSFER_TCP_SENDER_DELAY);

                TimeSpan remainingTimeSpan = TcpUtils.ComputeRemainingTime(startDateTime, bytesRead, totalBytesRead, fileDimension);
                fileTransfer.RemainingTime = remainingTimeSpan;

                // updating start time
                startDateTime = DateTime.Now;
            }

            fileStream.Close();

            if (fileTransfer.ContinueFileTransfer)
                fileTransfer.Status = TransferStatus.Completed;
            else
                fileTransfer.Status = TransferStatus.Canceled;
        }
        
        private void SendFileNodeDescription(TcpClient tcpClient, string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            long dimension = new FileInfo(filePath).Length;

            // preparing file node
            FileNode fileNode = new FileNode
            {
                Name = fileName,
                Dimension = dimension,
                MimeType = MimeMapping.GetMimeMapping(fileName),
                SenderUserName = Properties.Settings.Default.Name
            };

            string jsonFileNodeDescription = JsonConvert.SerializeObject(fileNode);

            TcpUtils.SendDescription(tcpClient, jsonFileNodeDescription);
        }
        
        public void SendDirectory()
        {
            TcpClient tcpClient = new TcpClient(_server, _port);

            // sending directory request
            TcpUtils.SendDirectoryRequest(tcpClient);

            // sending direcroty description
            DirectoryNode directoryNode = SendDirectoryNodeDescription(tcpClient);

            // gettin response
            string responseCommand = TcpUtils.ReceiveCommand(tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);
            if (!responseCommand.Equals(Constants.TRANSFER_TCP_ACCEPT))
            {
                ManageRefusedDirectory(_path);
                return;
            }

            // sending directory content
            SendDirectoryContent(directoryNode, _path);

            tcpClient.Close();
        }

        private void ManageRefusedDirectory(string path)
        {
            // preparing file node for file transfer
            FileNode fileNode = new FileNode
            {
                // name of the directory
                Name = new DirectoryInfo(path).Name,
                ReceiverUserName = _destinationUserName
            };

            // preparing file transfer for main window
            FileTransfer fileTransfer = new FileTransfer()
            {
                File = fileNode,
                Status = TransferStatus.Refused
            };

            InterfaceUtils.ShowBalloonTip(fileTransfer);
        }

        private DirectoryNode SendDirectoryNodeDescription(TcpClient tcpClient)
        {
            // preparing directory node
            DirectoryNode directoryNode = FilesUtils.BuildDirectoryNode(_path);
            directoryNode.SenderUserName = Properties.Settings.Default.Name;

            // sending directory node description
            string jsonDirectoryNodeDescription = JsonConvert.SerializeObject(directoryNode);
            TcpUtils.SendDescription(tcpClient, jsonDirectoryNodeDescription);

            return directoryNode;
        }

        private void SendDirectoryContent(DirectoryNode directoryNode, string directoryPath)
        {
            // sending each file in a separate thread
            foreach (var fileNode in directoryNode.FileNodes)
            {
                string filePath = directoryPath + "\\" + fileNode.Name;

                Thread thread = new Thread(() => SendFile(filePath));
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
                // recursive call in order to explore inner directories
                SendDirectoryContent(innerDirectoryNode, directoryPath + "\\" + innerDirectoryNode.DirectoryName);
        }
    }
}
