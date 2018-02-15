using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    class TCPSender
    {
        // client class

        private TcpClient _tcpClient;

        public TCPSender(String server)
        {
            _tcpClient = new TcpClient(server, Constants.TRANSFER_TCP_PORT);
        }

        public void SendFile(String filePath)
        {
            String fileName = Path.GetFileName(filePath);
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            long fileLenght = fileStream.Length;

            NetworkStream networkStream = _tcpClient.GetStream();

            Byte[] fileNameLengthData = new Byte[1];

            Byte[] fileNameData = System.Text.Encoding.UTF8.GetBytes(fileName);

            // FILE NAME LENGTH
            fileNameLengthData[0] = Convert.ToByte(fileNameData.Length);
            networkStream.Write(fileNameLengthData, 0, 1);

            // FILE NAME
            networkStream.Write(fileNameData, 0, fileNameData.Length);

            // FILE CONTENT LENGHT
            long fileContentLenght = new FileInfo(filePath).Length;
            Byte[] fileContentLenghtData = BitConverter.GetBytes(fileContentLenght);
            networkStream.Write(fileContentLenghtData, 0, fileContentLenghtData.Length);

            Byte[] fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];

            // FILE CONTENT
            // TODO continue developing
            while (fileStream.Read(fileContentBuffer, 0, fileContentBuffer.Length) > 0)
            {
                networkStream.Write(fileContentBuffer, 0, fileContentBuffer.Length);

                fileContentBuffer = new Byte[Constants.TRANSFER_TCP_BUFFER];
            }
        }
    }
}
