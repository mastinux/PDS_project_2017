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

namespace PDS_project_2017.Core
{
    class TCPSender
    {
        // client class

        private TcpClient _tcpClient;
        private String _path;

        public TCPSender(String server, String path)
        {
            _tcpClient = new TcpClient(server, Constants.TRANSFER_TCP_PORT);

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
                return;

            SendFileContent(filePath);
        }

        private void SendFileContent(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            NetworkStream networkStream = _tcpClient.GetStream();
            
            Byte[] fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            int bytesRead;

            // FILE CONTENT
            while ((bytesRead = fileStream.Read(fileContentBuffer, 0, fileContentBuffer.Length)) > 0)
            {
                networkStream.Write(fileContentBuffer, 0, bytesRead);
            }

            fileStream.Close();
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
    }
}
