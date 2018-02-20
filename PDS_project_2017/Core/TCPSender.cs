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

        private void SendDirectoryFile(string fileName)
        {
            TcpUtils.SendCommand(_tcpClient, Constants.TRANSFER_TCP_FILE);

            SendFileName(fileName);

            string response = TcpUtils.ReceiveCommand(_tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);

            if (!response.Equals(Constants.TRANSFER_TCP_ACCEPT))
                return;

            SendFileContent(fileName);
        }

        private void SendFileContent(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            NetworkStream networkStream = _tcpClient.GetStream();

            // FILE CONTENT LENGHT
            long fileContentLenght = new FileInfo(filePath).Length;
            Byte[] fileContentLenghtData = BitConverter.GetBytes(fileContentLenght);
            networkStream.Write(fileContentLenghtData, 0, fileContentLenghtData.Length);

            Byte[] fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            int bytesRead;

            // FILE CONTENT
            while ((bytesRead = fileStream.Read(fileContentBuffer, 0, fileContentBuffer.Length)) > 0)
            {
                networkStream.Write(fileContentBuffer, 0, bytesRead);
            }

            fileStream.Close();
        }

        private void SendFileName(string filePath)
        {
            String fileName = Path.GetFileName(filePath);

            TcpUtils.SendDescription(_tcpClient, fileName);
        }
        
        public void SendDirectory()
        {
            TcpUtils.SendCommand(_tcpClient, Constants.TRANSFER_TCP_DIRECTORY);

            DirectoryNode directoryNode = FilesUtils.ListDirectoryNode(_path);
            string jsonDirectoryNode = JsonConvert.SerializeObject(directoryNode);

            TcpUtils.SendDescription(_tcpClient, jsonDirectoryNode);

            string response = TcpUtils.ReceiveCommand(_tcpClient, Constants.TRANSFER_TCP_ACCEPT.Length);

            if (!response.Equals(Constants.TRANSFER_TCP_ACCEPT))
                return;

            SendDirectoryContent(directoryNode, _path);
        }

        private void SendDirectoryContent(DirectoryNode directoryNode, string directoryPath)
        {
            foreach (var fileName in directoryNode.FileNameNodes)
            {
                string filePath = directoryPath + "\\" + fileName;
                SendDirectoryFile(filePath);
            }

            foreach (var innerDirectoryNode in directoryNode.DirectoryNodes)
            {
                SendDirectoryContent(innerDirectoryNode, directoryPath + "\\" + innerDirectoryNode.DirectoryName);
            }
        }
    }
}
