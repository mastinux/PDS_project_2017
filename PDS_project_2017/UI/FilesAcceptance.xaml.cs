using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDS_project_2017.Core;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per FilesAcceptance.xaml
    /// </summary>
    public partial class FilesAcceptance : MetroWindow
    {
        private string _destinationDir;
        private bool _filesAccepted;
        
        public string DestinationDir { get => _destinationDir; }
        public bool AreFilesAccepted { get => _filesAccepted; }

        private void BaseConstructor()
        {
            InitializeComponent();

            _destinationDir = null;
            _filesAccepted = false;

            if (Properties.Settings.Default.UseDefaultDir == true)
            {
                DestinationDir_TextBox.Text = Properties.Settings.Default.DefaultDir;
                _destinationDir = Properties.Settings.Default.DefaultDir;
            }

            Top = Constants.RECEIVER_WINDOW_TOP;
            Left = Constants.RECEIVER_WINDOW_LEFT;
        }

        public FilesAcceptance(FileNode fileNode)
        {
            BaseConstructor();
            SetTitle(fileNode.Name);

            Filename_Label.Content = fileNode.Name;
            Size_Label.Content = ConvertToHumanReadableSize(fileNode.Dimension);
            Filetype_Label.Content = fileNode.MimeType;
        }

        private void SetTitle(string title)
        {
            Title = String.Format("Accept \"{0}\"", title);
        }

        public FilesAcceptance(DirectoryNode directoryNode)
        {
            BaseConstructor();
            SetTitle(directoryNode.DirectoryName);

            TreeView tv = new TreeView();
            PopulateTreeView(tv, directoryNode);
            dp.Children.Clear();
            dp.Children.Add(tv);
        }

        private void PopulateTreeView(TreeView tv, DirectoryNode dn)
        {
            tv.Items.Add(CreateTreeViewItem(dn));
            TreeViewItem tvi = (TreeViewItem)tv.Items.GetItemAt(0);
            tvi.IsExpanded = true;
        }

        private TreeViewItem CreateTreeViewItem(DirectoryNode dn)
        {
            TreeViewItem tvItem = new TreeViewItem() { Header = dn.DirectoryName };

            foreach(var directory in dn.DirectoryNodes)
            {
                tvItem.Items.Add(CreateTreeViewItem(directory));
            }

            foreach(var file in dn.FileNodes)
            {
                tvItem.Items.Add(new TreeViewItem()
                {
                    Header = file.Name + " (" + ConvertToHumanReadableSize(file.Dimension) + ")"
                });
            }

            return tvItem;
        }

        private string ConvertToHumanReadableSize(long fileDimension)
        {
            string[] suffixes = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

            int index = 0;

            while (fileDimension >= 1024)
            {
                index++;
                fileDimension /= 1024;
            }

            return String.Format("{0} {1}", fileDimension, suffixes[index]);
        }

        private void Accept_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_destinationDir == null)
            {
                this.ShowMessageAsync("Ops", "Choose destination directory");
                return;
            }

            _filesAccepted = true;

            this.Close();
        }

        private void Refuse_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DestinationDir_Button_Click(object sender, RoutedEventArgs e)
        { 
            Application.Current.Dispatcher.Invoke(() => { _destinationDir = UserUtils.RetrieveDirectoryLocation(); });

            DestinationDir_TextBox.Text = _destinationDir;
        }
    }
}
