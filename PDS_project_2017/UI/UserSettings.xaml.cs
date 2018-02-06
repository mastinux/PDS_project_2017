using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using PDS_project_2017.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace PDS_project_2017.UI
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
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
            Profile_Image.Source = LoadImage();
            Profile_Image.Stretch = Stretch.UniformToFill;
        }

        private void Image_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter =
            "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";
            fileDialog.FilterIndex = 1;

            fileDialog.ShowDialog();
            if (File.Exists(fileDialog.FileName))
            {
                //Bitmap img = new BitmapImage(new Uri(fileDialog.FileName));
                Bitmap img = (Bitmap)System.Drawing.Image.FromFile(fileDialog.FileName);
                SaveImage(img);
                Profile_Image.Source = LoadImage();
                Profile_Image.Stretch = Stretch.UniformToFill;
                //Profile_Image.Stretch = Stretch.Fill;
            }

        }

        public static BitmapImage LoadImage()
        {
            if (Properties.Settings.Default.Image.Trim().Length > 0 && Properties.Settings.Default.Image.Trim() != ""
                && Properties.Settings.Default.Image.Trim() != "\"\"")
            {
                string f = Properties.Settings.Default.Image;
                byte[] bytes = Convert.FromBase64String(f);
                System.IO.MemoryStream mem = new System.IO.MemoryStream(bytes);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = mem;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            return null;
        }

        private void SaveImage(Bitmap img)
        {
            //b = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            //using (Graphics g = Graphics.FromImage(b))
            //    g.Clear(Color.Red);
            Bitmap scaledImg = (Bitmap)ScaleImage(img, 100);

            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            scaledImg.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);

            string f = Convert.ToBase64String(mem.ToArray());
            Properties.Settings.Default.Image = f;
            Properties.Settings.Default.Save();

            mem.Close();
            mem.Dispose();
            mem = null;
        }

        public static System.Drawing.Image ScaleImage(System.Drawing.Image image, int maxHeight)
        {
            var ratio = (double)maxHeight / image.Height;

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
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
            //should stop announcing
            UdpListener.statusAvailableEvent.Reset();
        }

        private void Private_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PrivateMode = false;
            Properties.Settings.Default.Save();
            //should start announcing
            UdpListener.statusAvailableEvent.Set();
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
        }

        private void DefaultDir_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseDefaultDir = false;
            Properties.Settings.Default.Save();
        }

        private void DefaultDir_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result.ToString() == "Ok")
            {
                DefaultDir_TextBox.Text = dialog.FileName;
                Properties.Settings.Default.DefaultDir = dialog.FileName;
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Close();
        }

    }
}
