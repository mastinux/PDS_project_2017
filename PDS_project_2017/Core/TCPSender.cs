using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    class TCPSender
    {
        // client class

        private TcpClient _tcpClient;
        private String _filePath;

        public TCPSender(String server, String filePath)
        {
            _tcpClient = new TcpClient(server, Constants.TRANSFER_TCP_PORT);

            _filePath = filePath;
        }

        public void SendFile()
        {
            String filePath = _filePath;

            SendFileName(filePath);

            if (!ReceiveAcceptanceResponse())
                return;

            SendFileContent(filePath);
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
    }
}
