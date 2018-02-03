using MahApps.Metro.Controls;
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
            Profile_Image.Source = LoadImage();

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
    }
}
