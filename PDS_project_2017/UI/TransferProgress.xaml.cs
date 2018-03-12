using System;
using System.Windows;
using MahApps.Metro.Controls;
using PDS_project_2017.Core;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per TransferProgress.xaml
    /// </summary>
    public partial class TransferProgress : MetroWindow
    {
        public delegate void CancelTransferProcess();
        public CancelTransferProcess CancelTransferProcessEvent;
        
        public TransferProgress(FileNode fileNode, object o)
        {
            // TODO read some material about uwp
            // to be used in combination with wpf

            InitializeComponent();

            DataContext = this;
            
            UserName.Text = fileNode.SenderUserName;
            FileName.Text = fileNode.Name;
            ProgressBar.Value = 0;

            if (o.GetType() == typeof(TCPSender))
            {
                SetTcpSenderEvents((TCPSender) o);
                UserRole.Text = "Sending file to user: ";

                Top = Constants.SENDER_WINDOW_TOP;
                Left = Constants.SENDER_WINDOW_LEFT;
            }
            else if (o.GetType() == typeof(TcpReceiver))
            {
                SetTcpReceiverEvents((TcpReceiver) o);
                UserRole.Text = "Receiving file from user: ";

                Top = Constants.RECEIVER_WINDOW_TOP;
                Left = Constants.RECEIVER_WINDOW_LEFT;
            }
            else
                throw new Exception("bad type passed to TransferProgress()");
        }

        public void SetTcpSenderEvents(TCPSender tcpSender)
        {
            //tcpSender.UpdateProgressBarEvent += SetProgressBarValue;
            //tcpSender.UpdateRemainingTimeEvent += SetRemainingTimeValue;
        }

        public void SetTcpReceiverEvents(TcpReceiver tcpReceiver)
        {
            //tcpReceiver.UpdateProgressBarEvent += SetProgressBarValue;
            //tcpReceiver.UpdateRemainingTimeEvent += SetRemainingTimeValue;
        }

        public void SetIndex(int index)
        {
            Top = Top + index * Height + Constants.SENDER_WINDOW_OFFSET * index;
        }

        private void SetProgressBarValue(int value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ProgressBar.Value = value;
            }));
        }

        private void SetRemainingTimeValue(TimeSpan timeSpan)
        {
            string value = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                RemainingTime.Text = value;
            }));
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            CancelTransferProcessEvent();
        }
    }
}
