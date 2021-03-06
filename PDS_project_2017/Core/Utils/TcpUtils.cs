﻿using System;
using System.Net.Sockets;
using System.Text;

namespace PDS_project_2017.Core.Utils
{
    class TcpUtils
    {
        private static void SendCommand(TcpClient tcpClient, String command)
        {
            Byte[] data = new Byte[command.Length];

            data = Encoding.UTF8.GetBytes(command);

            tcpClient.GetStream().Write(data, 0, data.Length);
        }

        public static string ReceiveCommand(TcpClient tcpClient, int expectedCommandLenght)
        {
            Byte[] data = new Byte[expectedCommandLenght];
            
            tcpClient.GetStream().Read(data, 0, expectedCommandLenght);

            return Encoding.UTF8.GetString(data);
        }
        
        public static void SendRefuseResponse(TcpClient client)
        {
            SendCommand(client, Constants.TRANSFER_TCP_REFUSE);
        }

        public static void SendAcceptanceResponse(TcpClient client)
        {
            SendCommand(client, Constants.TRANSFER_TCP_ACCEPT);
        }

        public static void SendFileRequest(TcpClient client)
        {
            SendCommand(client, Constants.TRANSFER_TCP_FILE);
        }

        public static void SendDirectoryRequest(TcpClient client)
        {
            SendCommand(client, Constants.TRANSFER_TCP_DIRECTORY);
        }

        public static void SendDescription(TcpClient tcpClient, string description)
        {
            Byte[] descriptionData = Encoding.UTF8.GetBytes(description);

            NetworkStream networkStream = tcpClient.GetStream();

            // DESCRIPTION LENGTH
            Byte[] descriptionLengthData = BitConverter.GetBytes(description.Length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(descriptionLengthData);

            networkStream.Write(descriptionLengthData, 0, descriptionLengthData.Length);

            // DESCRIPTION
            networkStream.Write(descriptionData, 0, descriptionData.Length);
        }

        public static string ReceiveDescription(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();

            Byte[] descriptionLenghtData = new Byte[Constants.TRANSFER_TCP_INT_LEN];

            // FILE NAME LENGHT
            networkStream.Read(descriptionLenghtData, 0, Constants.TRANSFER_TCP_INT_LEN);

            Array.Reverse(descriptionLenghtData);

            int descriptionLenght = BitConverter.ToInt32(descriptionLenghtData, 0);

            Byte[] descriptionData = new byte[descriptionLenght];

            // FILE NAME
            networkStream.Read(descriptionData, 0, descriptionLenght);
            String description = Encoding.UTF8.GetString(descriptionData);

            return description;
        }
        
        public static TimeSpan ComputeRemainingTime(DateTime start, int currentPortion, long alreadyCompleted, long total)
        {
            DateTime end = DateTime.Now;
            TimeSpan timeSpanDifference = end - start;

            double transmissionSpeed = (double)currentPortion / timeSpanDifference.TotalSeconds;
            if (transmissionSpeed == 0 || double.IsNaN(transmissionSpeed))
                return TimeSpan.MaxValue;
            long remainingFileDimension = total - (long)alreadyCompleted;
            double remainingTime = (double)remainingFileDimension / transmissionSpeed;

            TimeSpan remainingTimeSpan = TimeSpan.FromSeconds(remainingTime);

            return remainingTimeSpan;
        }
    }
}
