using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using PDS_project_2017.Core.Entities;
using PDS_project_2017.Core.Utils;

namespace PDS_project_2017.Core
{
    public class TCPSender
    {
        // client class

        private TcpClient _tcpClient;
        private String _path;
        private int _index;
        private string _receiverUserName;

        public delegate void AddNewTransfer(FileTransfer transfer);
        public static event AddNewTransfer NewTransferEvent;

        public TCPSender(String server, int port, string receiverUserName, String path)
        {
            _tcpClient = new TcpClient(server, port);

            _receiverUserName = receiverUserName;

            _index = 0;
            
            _path = path;
        }

        public void SendFile()
        {
            SendDirectoryFile(_path);
            _tcpClient.Close();
        }

        private bool SendDirectoryFile(string filePath)
        {
            TcpUtils.SendFileRequest(_tcpClient);

            SendFileNodeDescription(filePath);

            string responseCommand = TcpUtils.ReceiveCommand(_tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);

            if (!responseCommand.Equals(Constants.TRANSFER_TCP_ACCEPT))
                return false;
            
            return SendFileContent(filePath);
        }

        private bool SendFileContent(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long fileDimension = fileStream.Length;
            NetworkStream networkStream = _tcpClient.GetStream();
            networkStream.WriteTimeout = Constants.TRANSFER_TCP_WRITE_TIMEOUT;
            
            FileNode fileNode = new FileNode();
            fileNode.Name = Path.GetFileName(filePath);
            fileNode.SenderUserName = Properties.Settings.Default.Name;
            fileNode.ReceiverUserName = _receiverUserName;

            FileTransfer fileTransfer = new FileTransfer()
            {
                File = fileNode,
                Progress = 0,
                ContinueFileTransfer = true,
                Status = TransferStatus.Pending,
                Sending = true
            };

            NewTransferEvent(fileTransfer);
            
            Byte[] fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            int bytesRead;
            long totalBytesRead = 0;
            DateTime baseDateTime = DateTime.Now;

            // FILE CONTENT
            while ((bytesRead = fileStream.Read(fileContentBuffer, 0, fileContentBuffer.Length)) > 0 && fileTransfer.ContinueFileTransfer)
            {
                try
                {
                    networkStream.Write(fileContentBuffer, 0, bytesRead);
                    
                }
                catch (IOException ioe)
                {
                    fileTransfer.Status = TransferStatus.Error;
                    return false;
                }
                
                totalBytesRead += bytesRead;
                double progress = (((double)totalBytesRead / (double)fileStream.Length) * 100);
                fileTransfer.Progress = progress;

                //TODO test purpose
                Thread.Sleep(Constants.TRANSFER_TCP_SENDER_DELAY + Constants.TRANSFER_TCP_SENDER_DELAY * _index);

                TimeSpan remainingTimeSpan = TcpUtils.ComputeRemainingTime(baseDateTime, bytesRead, totalBytesRead, fileDimension);
                fileTransfer.RemainingTime = remainingTimeSpan;
                baseDateTime = DateTime.Now;
            }

            fileStream.Close();

            if (!fileTransfer.ContinueFileTransfer)
            {
                fileTransfer.Status = TransferStatus.Canceled;

                return false;
            }
            else
            {
                fileTransfer.Status = TransferStatus.Completed;

                return true;
            }
        }
        
        private void SendFileNodeDescription(string filePath)
        {
            // FILE NAME
            String fileName = Path.GetFileName(filePath);
            // FILE DIMENSION
            long dimension = new FileInfo(filePath).Length;

            FileNode fileNode = new FileNode();
            fileNode.Name = fileName;
            fileNode.Dimension = dimension;
            fileNode.MimeType = MimeMapping.GetMimeMapping(fileName);
            fileNode.SenderUserName = Properties.Settings.Default.Name;

            string jsonFileNodeDescription = JsonConvert.SerializeObject(fileNode);

            TcpUtils.SendDescription(_tcpClient, jsonFileNodeDescription);
        }
        
        public void SendDirectory()
        {
            TcpUtils.SendDirectoryRequest(_tcpClient);

            DirectoryNode directoryNode = FilesUtils.BuildDirectoryNode(_path);
            directoryNode.SenderUserName = Properties.Settings.Default.Name;

            string jsonDirectoryNodeDescription = JsonConvert.SerializeObject(directoryNode);

            TcpUtils.SendDescription(_tcpClient, jsonDirectoryNodeDescription);

            string responseCommand = TcpUtils.ReceiveCommand(_tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);

            if (!responseCommand.Equals(Constants.TRANSFER_TCP_ACCEPT))
                return;

            SendDirectoryContent(directoryNode, _path);
        }

        private bool SendDirectoryContent(DirectoryNode directoryNode, string directoryPath)
        {
            foreach (var fileNode in directoryNode.FileNodes)
            {
                string filePath = directoryPath + "\\" + fileNode.Name;
                bool completed = SendDirectoryFile(filePath);

                if (!completed)
                    return false;
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
            {
                bool completed = SendDirectoryContent(innerDirectoryNode, directoryPath + "\\" + innerDirectoryNode.DirectoryName);

                if (!completed)
                    return false;
            }

            return true;
        }
    }
}
