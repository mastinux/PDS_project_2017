using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using MahApps.Metro.Controls;
using PDS_project_2017.UI;
using PDS_project_2017.Core;
using System.Threading;
using System.ComponentModel;
using PDS_project_2017.Core.Entities;
using System.Collections.ObjectModel;
using System.Web.Caching;
using System.Diagnostics;
using System.Linq;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // https://stackoverflow.com/questions/1472633/wpf-application-that-only-has-a-tray-icon
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        //private Dictionary<string, System.Drawing.Icon> appIcons = null;
        private System.Windows.Forms.MenuItem trayPrivateFlag = new System.Windows.Forms.MenuItem();

        private UdpListener udpListener;
        private ObservableCollection<FileTransfer> sendingTransferList;
        private ObservableCollection<FileTransfer> receivingTransferList;

        public MenuItem TrayPrivateFlag
        {
            get => trayPrivateFlag;
            set => trayPrivateFlag = value;
        }

        public UdpListener UdpListener
        {
            get => udpListener;
            set => udpListener = value;
        }

        public ObservableCollection<FileTransfer> SendingTransferList
        {
            get => sendingTransferList;
            set => sendingTransferList = value;
        }

        public ObservableCollection<FileTransfer> ReceivingTransferList
        {
            get => receivingTransferList;
            set => receivingTransferList = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.Name.CompareTo("") == 0)
            {
                UserSettings us = new UserSettings();
                us.ShowDialog(); //ShowDialog returns only when the window is closed
            }

            DataContext = this;

            sendingTransferList = new ObservableCollection<FileTransfer>();
            TCPSender.NewTransferEvent += AddNewTransfer;

            // udp socket listening for request
            udpListener = new UdpListener();

            // launching background thread
            Thread udpListenerThread = new Thread(UdpListener.listen);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();

            // tcp socket listening for file transfer
            TcpReceiver tcpReceiver = new TcpReceiver(Constants.TRANSFER_TCP_PORT);
            receivingTransferList = new ObservableCollection<FileTransfer>();
            TcpReceiver.NewTransferEvent += AddNewTransfer;

            // launching background thread
            Thread tcpReceiverThread = new Thread(tcpReceiver.Receive);
            tcpReceiverThread.SetApartmentState(ApartmentState.STA);
            tcpReceiverThread.IsBackground = true;
            tcpReceiverThread.Start();

            /*
            // test tcp socket listening for file transfer
            TcpReceiver testTcpReceiver = new TcpReceiver(Constants.TRANSFER_TCP_TEST_PORT);
            
            // launching background test thread
            Thread testTcpReceiverThread = new Thread(testTcpReceiver.Receive);
            testTcpReceiverThread.SetApartmentState(ApartmentState.STA);
            testTcpReceiverThread.IsBackground = true;
            testTcpReceiverThread.Start();
            */
        }
        
        private void AddNewTransfer(FileTransfer transfer)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
                new Action(() => {
                    if (transfer.Sending)
                        sendingTransferList.Add(transfer);
                    else
                        ReceivingTransferList.Add(transfer);
                }));
        }

        protected override void OnInitialized(EventArgs e)
        {
            Closing += OnClosing;

            initializeNotifyIcon();

            StartMinimized();

            base.OnInitialized(e);
        }

        private void StartMinimized()
        {
            this.WindowState = WindowState.Minimized;
            //this.ShowInTaskbar = false;
            //this.Topmost = false;
        }

        private void initializeNotifyIcon()
        {
            //appIcons = new Dictionary<string, System.Drawing.Icon>();
            //appIcons.Add("QuickLaunch", new System.Drawing.Icon(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\UI\images\LAN-Sharing.ico")));

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.MouseClick += notifyIcon_OnMouseClick;
            notifyIcon.Icon = Properties.Resources.LAN_Sharing;

            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipText =
                "The service has started, you can receive and send file from other users";
            notifyIcon.BalloonTipTitle = "Lan Sharing Application started";
            notifyIcon.Text = "Lan Sharing Application";

            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1000);

            System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
            //System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item3 = new System.Windows.Forms.MenuItem();

            System.Windows.Forms.ContextMenu cMenu = new System.Windows.Forms.ContextMenu();
            cMenu.MenuItems.Add(item1);
            // TODO temporary disabled
            // use Settings to enable/disable private mode
            cMenu.MenuItems.Add(TrayPrivateFlag);
            cMenu.MenuItems.Add(item3);

            notifyIcon.ContextMenu = cMenu;
            item1.Text = "Settings";
            TrayPrivateFlag.Text = "Private Mode";
            TrayPrivateFlag.Checked = Properties.Settings.Default.PrivateMode;
            item3.Text = "Exit";

            item1.Click += delegate
            {
                UserSettings us = new UserSettings();
                us.Show();
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
            item3.Click += delegate { App.Current.Shutdown(); };

        }

        private void notifyIcon_OnMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.ShowWindow();
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;

            // TODO need hwnd
            // Win32.Windows.SetWindowPos(this.Handle, Win32.Windows.Position.HWND_TOP, -1, -1, -1, -1, Win32.Windows.Options.SWP_NOSIZE | Win32.Windows.Options.SWP_NOMOVE);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
            WindowState = WindowState.Maximized;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer ft = (FileTransfer)item.CommandParameter;
            ft.ContinueFileTransfer = false;
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer ft = (FileTransfer)item.CommandParameter;
            if (ft.Sending)
                sendingTransferList.Remove(ft);
            else
                receivingTransferList.Remove(ft);
        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (System.Windows.Controls.Button)sender;
            FileTransfer ft = (FileTransfer)item.CommandParameter;
            Process.Start("explorer.exe", ft.SavingPath);
        }

        private void PurgeListFrom(ObservableCollection<FileTransfer> list, TransferStatus status)
        {
            List<FileTransfer> itemsToRemove = list.Where(x => x.Status == status).ToList();

            foreach (var item in itemsToRemove)
                list.Remove(item);
        }

        private void Clear_All_S_Completed_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(sendingTransferList, TransferStatus.Completed);
        }

        private void Clear_All_S_Canceled_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(sendingTransferList, TransferStatus.Canceled);
        }

        private void Clear_All_R_Completed_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(receivingTransferList, TransferStatus.Completed);
        }

        private void Clear_All_R_Error_Button_Click(object sender, RoutedEventArgs e)
        {
            PurgeListFrom(receivingTransferList, TransferStatus.Error);
        }
    }
}