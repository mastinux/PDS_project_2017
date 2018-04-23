using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using PDS_project_2017.Core;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.UI.Utils
{
    public class InterfaceUtils
    {
        // invoked when files are sent or received
        public static void ShowMainWindow(bool sending = true)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow mainWindow = App.Current.MainWindow as MainWindow;

                mainWindow.ShowWindow(sending);
            }));
        }

        public static void ShowBalloonTip(FileTransfer fileTransfer)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow mainWindow = App.Current.MainWindow as MainWindow;

                mainWindow.ShowBalloonTip(fileTransfer);
            }));
        }

        // used in UserSettings and FilesAcceptance
        public static String RetrieveDirectoryLocation()
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            CommonFileDialogResult result = dialog.ShowDialog();

            if (result.ToString() == "Ok")
                return dialog.FileName;

            return null;
        }

        public static BitmapImage LoadImage()
        {
            if (Properties.Settings.Default.Image == "")
                return LoadDefaultImage();

            string f = Properties.Settings.Default.Image;
            byte[] bytes = Convert.FromBase64String(f);

            MemoryStream mem = new MemoryStream(bytes);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = mem;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static BitmapImage LoadDefaultImage()
        {
            return ConvertBitmapToBitmapImage(Properties.Resources.DefaultProfileImage);
        }
        
        private static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        
        public static void SaveImage(Bitmap img)
        {
            Bitmap scaledImg = (Bitmap)ScaleImage(img, 100);

            MemoryStream mem = new MemoryStream();
            scaledImg.Save(mem, ImageFormat.Jpeg);

            string base64String = Convert.ToBase64String(mem.ToArray());
            Properties.Settings.Default.Image = base64String;
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

        public static string LoadName()
        {
            return Properties.Settings.Default.Name;
        }

        public static TreeView CreateTreeView(DirectoryNode dn)
        {
            TreeView treeView = new TreeView();

            treeView.Items.Add(CreateTreeViewItem(dn));

            TreeViewItem tvi = (TreeViewItem)treeView.Items.GetItemAt(0);
            tvi.IsExpanded = true;

            return treeView;
        }

        private static TreeViewItem CreateTreeViewItem(DirectoryNode dn)
        {
            TreeViewItem tvItem = new TreeViewItem() { Header = GenerateHeader(dn), FontWeight = FontWeights.Bold };

            foreach (var directory in dn.DirectoryNodes)
                tvItem.Items.Add(CreateTreeViewItem(directory));

            foreach (var file in dn.FileNodes)
            {
                tvItem.Items.Add(new TreeViewItem()
                {
                    Header = file.Name + " (" + ConvertToHumanReadableSize(file.Dimension) + ")",
                    FontWeight = FontWeights.Regular
                });
            }

            return tvItem;
        }

        public static string ConvertToHumanReadableSize(long fileDimension)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            int index = 0;

            while (fileDimension >= 1024)
            {
                index++;
                fileDimension /= 1024;
            }

            return String.Format("{0} {1}", fileDimension, suffixes[index]);
        }

        private static string GenerateHeader(DirectoryNode directoryNode)
        {
            string header = directoryNode.DirectoryName + " (";

            int directoriesInDirectory = directoryNode.DirectoryNodes.Count;
            int filesInDirectory = directoryNode.FileNodes.Count;

            if (directoriesInDirectory == 0 && filesInDirectory == 0)
            {
                header += "empty)";
            }
            else
            {
                string directoriesDescription = directoriesInDirectory + " directories";
                string filesDescription = filesInDirectory + " files";

                if (directoriesInDirectory > 0)
                {
                    header += directoriesDescription;

                    if (filesInDirectory > 0)
                        header += ", " + filesDescription;
                }
                else
                {
                    header += filesDescription;
                }

                header += ")";
            }

            return header;
        }
    }
}
