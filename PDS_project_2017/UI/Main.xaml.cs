using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using PDS_project_2017.Core;
using PDS_project_2017.Core.Entities;
using Application = System.Windows.Application;

namespace PDS_project_2017.UI
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private NotifyIcon _notifyIcon = null;

        public MenuItem TrayPrivateFlag { get; set; }

        public UdpListener UdpListener { get; set; }

        public ObservableCollection<FileTransfer> SendingTransferList { get; set; }

        public ObservableCollection<FileTransfer> ReceivingTransferList { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.Name.CompareTo("") == 0)
            {
                // first application run
                UserSettings us = new UserSettings();
                us.ShowDialog(); //ShowDialog returns only when the window is closed
            }

            DataContext = this;

            SendingTransferList = new ObservableCollection<FileTransfer>();
            // sorting sending fileTransfers
            ICollectionView sendingTransferListView = CollectionViewSource.GetDefaultView(SendingTransferList);
            sendingTransferListView.SortDescriptions.Add(new SortDescription("Status", ListSortDirection.Ascending));
            sendingTransferListView.SortDescriptions.Add(new SortDescription("ManagementDateTime", ListSortDirection.Ascending));
            
            TCPSender.NewTransferEvent += AddNewTransfer;

            // udp socket listening for request
            UdpListener = new UdpListener();

            // launching background thread
            Thread udpListenerThread = new Thread(UdpListener.Listen);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();

            ReceivingTransferList = new ObservableCollection<FileTransfer>();
            // sorting receiving fileTransfers
            ICollectionView receivingTransferListView = CollectionViewSource.GetDefaultView(ReceivingTransferList);
            receivingTransferListView.SortDescriptions.Add(new SortDescription("Status", ListSortDirection.Ascending));
            receivingTransferListView.SortDescriptions.Add(new SortDescription("ManagementDateTime", ListSortDirection.Ascending));

            // tcp socket listening for file transfer
            TcpReceiver tcpReceiver = new TcpReceiver(Constants.TRANSFER_TCP_PORT);
            TcpReceiver.NewTransferEvent += AddNewTransfer;

            // launching background thread
            Thread tcpReceiverThread = new Thread(tcpReceiver.Receive);
            tcpReceiverThread.SetApartmentState(ApartmentState.STA);
            tcpReceiverThread.IsBackground = true;
            tcpReceiverThread.Start();

            // retrieving old sending file transfers
            if (Properties.Settings.Default.SendingTransferFiles != "")
            {
                string transfers = Properties.Settings.Default.SendingTransferFiles;

                foreach (var fileTransfer in JsonConvert.DeserializeObject<ObservableCollection<FileTransfer>>(transfers))
                {
                    if (fileTransfer.Status == TransferStatus.Pending)
                        fileTransfer.Status = TransferStatus.Error;

                    SendingTransferList.Add(fileTransfer);
                }
            }

            // retrieving old receiving file transfers
            if (Properties.Settings.Default.ReceivingTransferFiles != "")
            {
                string transfers = Properties.Settings.Default.ReceivingTransferFiles;

                foreach (var fileTransfer in JsonConvert.DeserializeObject<ObservableCollection<FileTransfer>>(transfers))
                {
                    if (fileTransfer.Status == TransferStatus.Pending)
                        fileTransfer.Status = TransferStatus.Error;

                    ReceivingTransferList.Add(fileTransfer);
                }
            }
        }

        private void AddNewTransfer(FileTransfer transfer)
        {
            transfer.StatusChangedEvent += ShowBalloonTip;
            // forcing calling of status change event, in order to show balloon tip
            transfer.Status = transfer.Status;

            Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    if (transfer.Sending)
                        SendingTransferList.Add(transfer);
                    else
                        ReceivingTransferList.Add(transfer);
                }));
        }

        public void ShowBalloonTip(FileTransfer filetransfer)
        {
            string title = ""; // filename
            string text = ""; // operation on file

            switch (filetransfer.Status)
            {
                case TransferStatus.Pending:
                    title += filetransfer.File.Name;
                    text += (filetransfer.Sending ? "Sending" : "Receiving") + " " + filetransfer.File.Name;
                    break;
                case TransferStatus.Completed:
                    title += filetransfer.File.Name;
                    text += filetransfer.File.Name + " " + (filetransfer.Sending ? "sent" : "received");
                    break;
                case TransferStatus.Canceled:
                    title += filetransfer.File.Name;
                    text += (filetransfer.Sending ? "Sending" : "Receiving") + " of " + filetransfer.File.Name + " canceled";
                    break;
                case TransferStatus.Error:
                    title += filetransfer.File.Name;
                    text += "Error while " + (filetransfer.Sending ? "sending" : "receiving") + " " + filetransfer.File.Name;
                    break;
                case TransferStatus.Refused:
                    title += filetransfer.File.Name;
                    text += filetransfer.File.Name + " refused by " + filetransfer.File.ReceiverUserName;
                    break;
            }
            
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = text;

            _notifyIcon.ShowBalloonTip(Constants.BALLOONTIP_DELAY);
        }

        protected override void OnInitialized(EventArgs e)
        {
            Closing += HideWindow;

            InitializeNotifyIcon();

            StartMinimized();

            base.OnInitialized(e);
        }

        private void StartMinimized()
        {
            Hide();

            WindowState = WindowState.Minimized;
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.MouseClick += NotifyIcon_OnMouseClick;
            _notifyIcon.Icon = Properties.Resources.LAN_Sharing;

            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.BalloonTipText = Properties.Resources.Startup_Baloon_Tip_Text;
            _notifyIcon.BalloonTipTitle = Properties.Resources.Startup_Baloon_Tip_Title;
            _notifyIcon.Text = Properties.Resources.Application_Name;

            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(Constants.BALLOONTIP_DELAY);

            MenuItem item1 = new MenuItem();
            TrayPrivateFlag = new MenuItem();
            MenuItem item3 = new MenuItem();

            ContextMenu cMenu = new ContextMenu();
            cMenu.MenuItems.Add(item1);
            cMenu.MenuItems.Add(TrayPrivateFlag);
            cMenu.MenuItems.Add(item3);

            _notifyIcon.ContextMenu = cMenu;
            item1.Text = "Settings";
            TrayPrivateFlag.Text = "Private Mode";
            TrayPrivateFlag.Checked = Properties.Settings.Default.PrivateMode;
            item3.Text = "Exit";

            item1.Click += delegate
            {
                ShowOrCreateUserSettingsWindow();
            };

            TrayPrivateFlag.Click += delegate
            {
                if (Properties.Settings.Default.PrivateMode)
                {
                    Properties.Settings.Default.PrivateMode = false;
                    TrayPrivateFlag.Checked = false;
                    UdpListener.SetStatusAvailableEvent();
                }
                else
                {
                    Properties.Settings.Default.PrivateMode = true;
                    TrayPrivateFlag.Checked = true;
                    UdpListener.ResetStatusAvailableEvent();
                }

                Properties.Settings.Default.Save();
            };

            item3.Click += delegate
            {
                StoreHistory();

                App.Current.Shutdown();
            };

        }

        private void ShowOrCreateUserSettingsWindow()
        {
            // retrieving existing user settings window
            var existingWindow = Application.Current.Windows.Cast<Window>().SingleOrDefault(w => w is UserSettings);

            if (existingWindow != null)
            {
                // activating
                existingWindow.Activate();
            }
            else
            {
                // creating and showing
                UserSettings us = new UserSettings();
                us.Show();
            }
        }

        private void StoreHistory()
        {
            Properties.Settings.Default.SendingTransferFiles = JsonConvert.SerializeObject(SendingTransferList);
            Properties.Settings.Default.ReceivingTransferFiles = JsonConvert.SerializeObject(ReceivingTransferList);
            Properties.Settings.Default.Save();
        }

        private void NotifyIcon_OnMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowWindow();
        }

        public void ShowWindow(bool sending = true)
        {
            if (sending)
                TabControl.SelectedItem = SendingTabItem;
            else
                TabControl.SelectedItem = ReceivingTabItem;

            ShowInTaskbar = true;
            Show();
            Activate();
            WindowState = WindowState.Normal;
        }

        private void HideWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            ShowInTaskbar = false;
            Hide();
            WindowState = WindowState.Maximized;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer fileTransfer = (FileTransfer)item.CommandParameter;

            fileTransfer.ContinueFileTransfer = false;
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer fileTransfer = (FileTransfer)item.CommandParameter;

            if (fileTransfer.Sending)
                SendingTransferList.Remove(fileTransfer);
            else
                ReceivingTransferList.Remove(fileTransfer);
        }

        private void Open_File_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer fileTransfer = (FileTransfer)item.CommandParameter;

            string filePath = fileTransfer.DestinationDirectoryPath + "\\" + fileTransfer.File.Name;

            if (File.Exists(filePath))
                Process.Start("explorer.exe", filePath);
            else
                fileTransfer.Status = TransferStatus.Removed;
        }

        private void Open_Directory_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer fileTransfer = (FileTransfer)item.CommandParameter;

            if (Directory.Exists(fileTransfer.DestinationDirectoryPath))
                Process.Start("explorer.exe", fileTransfer.DestinationDirectoryPath);
            else
                fileTransfer.Status = TransferStatus.Removed;
        }

        private void PurgeListFrom(ObservableCollection<FileTransfer> list, TransferStatus status)
        {
            List<FileTransfer> itemsToRemove = list.Where(x => x.Status == status).ToList();

            foreach (var item in itemsToRemove)
                list.Remove(item);
        }

        private void Clear_All_S_Completed_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(SendingTransferList, TransferStatus.Completed);
        }

        private void Clear_All_S_Canceled_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(SendingTransferList, TransferStatus.Canceled);
        }

        private void Clear_All_S_Error_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(SendingTransferList, TransferStatus.Error);
        }

        private void Clear_All_R_Completed_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(ReceivingTransferList, TransferStatus.Completed);
        }

        private void Clear_All_R_Canceled_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(ReceivingTransferList, TransferStatus.Canceled);
        }

        private void Clear_All_R_Removed_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(ReceivingTransferList, TransferStatus.Removed);
        }

        private void Clear_All_R_Error_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(ReceivingTransferList, TransferStatus.Error);
        }
    }
}