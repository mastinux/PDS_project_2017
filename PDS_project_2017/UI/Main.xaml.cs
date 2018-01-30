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
using System.Threading;
using PDS_project_2017.Core;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // https://stackoverflow.com/questions/1472633/wpf-application-that-only-has-a-tray-icon
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private Dictionary<string, System.Drawing.Icon> appIcons = null;


        protected override void OnInitialized(EventArgs e)
        {
            Trace.WriteLine(this.GetType().Name + ": oninitialized");

            base.OnInitialized(e);

            appIcons = new Dictionary<string, System.Drawing.Icon>();
            appIcons.Add("QuickLaunch", new System.Drawing.Icon(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\UI\images\LAN-Sharing.ico")));

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += notifyIcon_Click;
            //notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Icon = appIcons["QuickLaunch"];
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Trace.WriteLine(this.GetType().Name + ": notifyicon_click");

        }
        
        public MainWindow()
        {
            Trace.WriteLine(this.GetType().Name + ": constructor");

            InitializeComponent();

            notifyIcon.Visible = true;
            /*
            FilesAcceptance filesAcceptance = new FilesAcceptance();
            filesAcceptance.Show();
            
            UsersSelection usersSelection = new UsersSelection();
            usersSelection.Show();

            TransferProgress transferProgress = new TransferProgress();
            transferProgress.Show();
            */

            // udp socket listening for request
            UdpListener udpListener = new UdpListener();

            // launching background thread
            Thread udpListenerThread = new Thread(udpListener.listen);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();

            UdpRequester udpRequester = new UdpRequester();
            udpRequester.request();
        }

        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButton1.IsChecked == true)
            {
                MessageBox.Show("Hello.");
            }
            else
            {
                RadioButton2.IsChecked = true;
                MessageBox.Show("Goodbye.");
            }
        }
        */
    }
}
