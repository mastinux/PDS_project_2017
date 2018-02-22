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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using MahApps.Metro.Controls;
using PDS_project_2017.UI;
using PDS_project_2017.Core;
using System.Threading;
using System.ComponentModel;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;

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
        
        public MainWindow()
        {
            // TODO update notify icon menu for private mode when usersettings are changed

            InitializeComponent();

            if (Properties.Settings.Default.Name.CompareTo("") == 0)
            {
                UserSettings us = new UserSettings();
                us.ShowDialog(); //ShowDialog returns only when the window is closed
            }

            // udp socket listening for request
            UdpListener udpListener = new UdpListener();

            // launching background thread
            Thread udpListenerThread = new Thread(udpListener.listen);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();

            // tcp socket listening for file transfer
            TcpReceiver tcpReceiver = new TcpReceiver();

            // launching background thread
            Thread tcpReceiverThread = new Thread(tcpReceiver.Receive);
            // https://stackoverflow.com/questions/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this
            tcpReceiverThread.SetApartmentState(ApartmentState.STA);
            tcpReceiverThread.IsBackground = true;
            tcpReceiverThread.Start();
        }

        protected override void OnInitialized(EventArgs e)
        {
            StateChanged += OnStateChanged;
            Closing += OnClosing;
            //Loaded += OnLoaded;

            initializeNotifyIcon();

            this.startMinimized();
            
            base.OnInitialized(e);
        }

        private void startMinimized()
        {
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Minimized;

            // not showing main window on startup
            //this.Show();

            this.ShowInTaskbar = false;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            //Hide();
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
            System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item3 = new System.Windows.Forms.MenuItem();
     
            System.Windows.Forms.ContextMenu cMenu = new System.Windows.Forms.ContextMenu();
            cMenu.MenuItems.Add(item1);
            cMenu.MenuItems.Add(item2);
            cMenu.MenuItems.Add(item3);

            notifyIcon.ContextMenu = cMenu;
            item1.Text = "Settings";
            item2.Text = "Private Mode";
            item2.Checked = Properties.Settings.Default.PrivateMode;
            item3.Text = "Exit";
    
            item1.Click += delegate { UserSettings us = new UserSettings();  us.Show();  };
            item2.Click += delegate
            {
                if (Properties.Settings.Default.PrivateMode)
                {
                    Properties.Settings.Default.PrivateMode = false;
                    item2.Checked = false;
                    UdpListener.SetStatusAvailableEvent();
                }
                else
                {
                    Properties.Settings.Default.PrivateMode = true;
                    item2.Checked = true;
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

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Topmost = false;
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
                this.Topmost = true;
            }
        }

        private void ShowWindow()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.Show();
            // TODO need hwnd
            // Win32.Windows.SetWindowPos(this.Handle, Win32.Windows.Position.HWND_TOP, -1, -1, -1, -1, Win32.Windows.Options.SWP_NOSIZE | Win32.Windows.Options.SWP_NOMOVE);
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            // initing user selection class
            UsersSelection usersSelection = new UsersSelection("Test");
            usersSelection.Show();
        }
    }
}
