using MahApps.Metro.Controls;
using PDS_project_2017.Core;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using PDS_project_2017.UI.Utils;


namespace PDS_project_2017.UI
{
    public partial class UserSettings : MetroWindow
    {
        public UserSettings()
        {
            InitializeComponent();

            Name_TextBox.Text = Properties.Settings.Default.Name;
            Private_CheckBox.IsChecked = Properties.Settings.Default.PrivateMode;
            AutoAccept_CheckBox.IsChecked = Properties.Settings.Default.AutoAccept;
            DefaultDir_CheckBox.IsChecked = Properties.Settings.Default.UseDefaultDir;
            DefaultDir_TextBox.Text = Properties.Settings.Default.DefaultDir;
            Profile_Image.Source = InterfaceUtils.LoadImage();
        }

        private void Change_Image_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog =
                new OpenFileDialog
                {
                    Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*",
                    FilterIndex = 1
                };

            fileDialog.ShowDialog();

            if (File.Exists(fileDialog.FileName))
            {
                Bitmap img = (Bitmap)System.Drawing.Image.FromFile(fileDialog.FileName);
                InterfaceUtils.SaveImage(img);

                Profile_Image.Source = InterfaceUtils.LoadImage();
            }

            Properties.Settings.Default.Save();
        }

        private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;

            Properties.Settings.Default.Name = textBox.Text;
            Properties.Settings.Default.Save();
        }

        private void Private_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PrivateMode = true;
            Properties.Settings.Default.Save();

            UpdateTrayandAnnouncing(true);
        }

        private void Private_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PrivateMode = false;
            Properties.Settings.Default.Save();

            UpdateTrayandAnnouncing(false);
        }

        private void UpdateTrayandAnnouncing(bool privateStatus)
        {
            //Update only if it's not the first time launch
            MainWindow mainWindow;

            mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(w => w.IsInitialized);

            if(mainWindow != null)
            {
                mainWindow.TrayPrivateFlag.Checked = privateStatus;
                if (mainWindow.UdpListener != null)
                {
                    if (privateStatus)
                        UdpListener.ResetStatusAvailableEvent(); //should stop announcing
                    else
                        UdpListener.SetStatusAvailableEvent(); //should start announcing
                }
            }
        }

        private void AutoAccept_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoAccept = true;
            Properties.Settings.Default.Save();
        }

        private void AutoAccept_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoAccept = false;
            Properties.Settings.Default.Save();
        }

        private void DefaultDir_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseDefaultDir = true;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.DefaultDir == null)
                DefaultDir_Button_Click(sender, e);
        }

        private void DefaultDir_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseDefaultDir = false;
            Properties.Settings.Default.DefaultDir = null;
            Properties.Settings.Default.Save();

            DefaultDir_TextBox.Text = null;
            AutoAccept_CheckBox.IsChecked = false;
        }

        private void DefaultDir_Button_Click(object sender, RoutedEventArgs e)
        {
            string defaultDir = InterfaceUtils.RetrieveDirectoryLocation();

            Properties.Settings.Default.DefaultDir = defaultDir;
            Properties.Settings.Default.Save();

            DefaultDir_TextBox.Text = defaultDir;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Remove_Image_Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Image = "";
            Properties.Settings.Default.Save();

            Profile_Image.Source = InterfaceUtils.LoadDefaultImage();
        }
    }
}
