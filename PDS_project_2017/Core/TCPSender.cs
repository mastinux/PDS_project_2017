using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using PDS_project_2017.Core.Entities;
using PDS_project_2017.Core.Utils;
using PDS_project_2017.UI;

namespace PDS_project_2017.Core
{
    public class TCPSender
    {
        // client class

        private TcpClient _tcpClient;
        private String _path;
        private string _userName;
        private int _index;

        private bool _continueTransferProcess;

        public delegate void UpdateProgressBarValue(int value);
        public event UpdateProgressBarValue UpdateProgressBarEvent;

        public delegate void UpdateRemainingTimeValue(TimeSpan timeSpan);
        public event UpdateRemainingTimeValue UpdateRemainingTimeEvent;

        public TCPSender(String server, int port, string userName, String path)
        {
            _tcpClient = new TcpClient(server, port);

            _userName = userName;
            _path = path;
        }

        public void SendFile()
        {
            SendDirectoryFile(_path);
        }

        private void SendDirectoryFile(string filePath)
        {
            TcpUtils.SendFileRequest(_tcpClient);

            SendFileNodeDescription(filePath);

            string responseCommand = TcpUtils.ReceiveCommand(_tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);

            if (!responseCommand.Equals(Constants.TRANSFER_TCP_ACCEPT))
            {
                Console.WriteLine("file refused");
                return;
            }

            SendFileContent(filePath);
        }

        private void SendFileContent(string filePath)
        {
            // TRANSFER PROGRESS
            TransferProgress transferProgressWindow = InitTransferProgressWindow(filePath);

            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long fileDimension = fileStream.Length;
            NetworkStream networkStream = _tcpClient.GetStream();
            
            Byte[] fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            int bytesRead;
            int totalBytesRead = 0;
            _continueTransferProcess = true;

            DateTime baseDateTime = DateTime.Now;

            // FILE CONTENT
            while ((bytesRead = fileStream.Read(fileContentBuffer, 0, fileContentBuffer.Length)) > 0 && _continueTransferProcess)
            {
                networkStream.Write(fileContentBuffer, 0, bytesRead);
                totalBytesRead += bytesRead;
                UpdateProgressBarEvent((int)(((float) totalBytesRead / (float) fileStream.Length) * 100));
                Thread.Sleep(150 * _index);

                DateTime currentDateTime = DateTime.Now;
                TimeSpan timeSpanDifference = currentDateTime - baseDateTime;
                double transmissionSpeed = (double)bytesRead / timeSpanDifference.TotalSeconds;
                long remainingFileDimension = fileDimension - (long)totalBytesRead;
                double remainingTime = (double)remainingFileDimension / transmissionSpeed;
                TimeSpan remainingTimeSpan = TimeSpan.FromSeconds(remainingTime);
                UpdateRemainingTimeEvent(remainingTimeSpan);
                
                baseDateTime = currentDateTime;
            }

            if (_continueTransferProcess == false)
            {
                Console.WriteLine("transfer process cancelled");
            }

            fileStream.Close();

            CloseTransferProgressWindow(transferProgressWindow);
        }

        private void CloseTransferProgressWindow(TransferProgress transferProgressWindow)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                transferProgressWindow.Close();
            }));
        }

        private TransferProgress InitTransferProgressWindow(string filePath)
        {
            TransferProgress transferProgressWindow = null;
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                transferProgressWindow = new TransferProgress(_userName, Path.GetFileName(filePath), this);
                transferProgressWindow.SetIndex(_index);
                transferProgressWindow.CancelTransferProcessEvent = () => _continueTransferProcess = false;
                transferProgressWindow.Show();
            }));

            return transferProgressWindow;
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

            string jsonFileNodeDescription = JsonConvert.SerializeObject(fileNode);

            TcpUtils.SendDescription(_tcpClient, jsonFileNodeDescription);
        }
        
        public void SendDirectory()
        {
            TcpUtils.SendDirectoryRequest(_tcpClient);

            DirectoryNode directoryNode = FilesUtils.BuildDirectoryNode(_path);
            string jsonDirectoryNodeDescription = JsonConvert.SerializeObject(directoryNode);

            TcpUtils.SendDescription(_tcpClient, jsonDirectoryNodeDescription);

            string responseCommand = TcpUtils.ReceiveCommand(_tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);

            if (!responseCommand.Equals(Constants.TRANSFER_TCP_ACCEPT))
                return;

            SendDirectoryContent(directoryNode, _path);
        }

        private void SendDirectoryContent(DirectoryNode directoryNode, string directoryPath)
        {
            foreach (var fileNode in directoryNode.FileNodes)
            {
                string filePath = directoryPath + "\\" + fileNode.Name;
                SendDirectoryFile(filePath);
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
            {
                SendDirectoryContent(innerDirectoryNode, directoryPath + "\\" + innerDirectoryNode.DirectoryName);
            }
        }

        public void SetIndex(int i)
        {
            _index = i;
        }
    }
}
