using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDS_project_2017.Core;
using PDS_project_2017.Core.Entities;
using PDS_project_2017.UI.Utils;

namespace PDS_project_2017.UI
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

            if (Properties.Settings.Default.UseDefaultDir)
            {
                DestinationDir_TextBox.Text = Properties.Settings.Default.DefaultDir;
                _destinationDir = Properties.Settings.Default.DefaultDir;
            }

            Top = Constants.RECEIVER_WINDOW_TOP;
            Left = Constants.RECEIVER_WINDOW_LEFT;
        }

        private void SetTitle(string title)
        {
            Title = String.Format("Accept \"{0}\"", title);
        }

        public FilesAcceptance(FileNode fileNode)
        {
            BaseConstructor();
            SetTitle(fileNode.Name);

            Filename_Label.Content = fileNode.Name;
            Size_Label.Content = InterfaceUtils.ConvertToHumanReadableSize(fileNode.Dimension);
            Filetype_Label.Content = fileNode.MimeType;
        }

        public FilesAcceptance(DirectoryNode directoryNode)
        {
            BaseConstructor();
            SetTitle(directoryNode.DirectoryName);

            TreeView tv = InterfaceUtils.CreateTreeView(directoryNode);
            dp.Children.Clear();
            dp.Children.Add(tv);
        }

        private void Accept_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_destinationDir == null)
            {
                this.ShowMessageAsync("Ops", "Choose destination directory");
                return;
            }

            _filesAccepted = true;

            InterfaceUtils.ShowMainWindow();
            
            Close();
        }

        private void Refuse_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DestinationDir_Button_Click(object sender, RoutedEventArgs e)
        { 
            Application.Current.Dispatcher.Invoke(() => { _destinationDir = InterfaceUtils.RetrieveDirectoryLocation(); });

            DestinationDir_TextBox.Text = _destinationDir;
        }
    }
}
