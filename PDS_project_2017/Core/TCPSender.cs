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
            InternalSendFile(_path);
        }

        private void InternalSendFile(string fileName)
        {
            SendFileName(fileName);

            if (!ReceiveAcceptanceResponse())
                return;

            SendFileContent(fileName);
        }

        private bool ReceiveAcceptanceResponse()
        {
            Byte[] data = new Byte[Constants.TRANSFER_TCP_COMMAND_LEN];

            int n = _tcpClient.GetStream().Read(data, 0, Constants.TRANSFER_TCP_COMMAND_LEN);

            String response = Encoding.UTF8.GetString(data);

            return response.Equals(Constants.TRANSFER_TCP_ACCEPT);
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

            Byte[] fileNameLengthData = new Byte[Constants.TRANSFER_TCP_FILE_NAME_LEN];
            Byte[] fileNameData = System.Text.Encoding.UTF8.GetBytes(fileName);
            
            NetworkStream networkStream = _tcpClient.GetStream();

            // FILE NAME LENGTH
            fileNameLengthData[0] = Convert.ToByte(fileNameData.Length);
            networkStream.Write(fileNameLengthData, 0, 1);

            // FILE NAME
            networkStream.Write(fileNameData, 0, fileNameData.Length);
        }

        public void SendDirectory()
        {
            //TreeView treeView = FilesUtils.ListDirectory(_path);
            DirectoryNode directoryNode = FilesUtils.ListDirectoryNode(_path);
            directoryNode.Print();

            string jsonDirectoryNode = JsonConvert.SerializeObject(directoryNode);

            // TODO send json then wait confirmation for whole hieracy and send all

            //Console.WriteLine(jsonDirectoryNode);
            Console.WriteLine("----------------------------------------------");

            DirectoryNode reverseDirectoryNode = JsonConvert.DeserializeObject<DirectoryNode>(jsonDirectoryNode);
            reverseDirectoryNode.Print();
        }
    }
}
