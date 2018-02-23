using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using PDS_project_2017.Core;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per TransferProgress.xaml
    /// </summary>
    public partial class TransferProgress : MetroWindow
    {
        public delegate void CancelTransferProcess();
        public CancelTransferProcess CancelTransferProcessEvent;
        
        public TransferProgress(string userName, string fileName)
        {
            // TODO read some material about uwp
            // to be used in combination with wpf

            InitializeComponent();

            DataContext = this;
            
            UserName.Text = userName;
            FileName.Text = fileName;
            ProgressBar.Value = 0;

            TCPSender.UpdateProgressBarEvent += SetProgressBarValue;
            TCPSender.UpdateRemainingTimeEvent += SetRemainingTimeValue;

            Top = Constants.SENDER_WINDOW_TOP;
            Left = Constants.SENDER_WINDOW_LEFT;
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
