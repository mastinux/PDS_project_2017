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
            Trace.WriteLine("oninitialized");
      
            StateChanged += OnStateChanged;
            //Loaded += OnLoaded;
              

            appIcons = new Dictionary<string, System.Drawing.Icon>();
            appIcons.Add("QuickLaunch", new System.Drawing.Icon(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\UI\images\LAN-Sharing.ico")));

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += notifyIcon_Click;
            //notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Icon = appIcons["QuickLaunch"];
            notifyIcon.Visible = true;

            this.startMinimized();
            
            base.OnInitialized(e);
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("notifyicon_click");
            this.ShowWindow();

        }

        private void startMinimized()
        {
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Minimized;
            this.Show();
            this.ShowInTaskbar = false;
            notifyIcon.Visible = true;
        }


        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Topmost = false;
                this.ShowInTaskbar = false;
                notifyIcon.Visible = true;
            }
            else
            {
                notifyIcon.Visible = true;
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

        public MainWindow()
        {
            Trace.WriteLine("constructor");

            InitializeComponent();

            //notifyIcon.Visible = true;
            /*
            FilesAcceptance filesAcceptance = new FilesAcceptance();
            filesAcceptance.Show();

            UsersSelection usersSelection = new UsersSelection();
            usersSelection.Show();

            TransferProgress transferProgress = new TransferProgress();
            transferProgress.Show();
            */
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
