using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
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
        }

        public FilesAcceptance(FileNode fileNode)
        {
            BaseConstructor();
            SetTitle(fileNode.Name);
        }

        private void SetTitle(string title)
        {
            Title = String.Format("Accept \"{0}\"", title);
        }

        public FilesAcceptance(DirectoryNode directoryNode)
        {
            BaseConstructor();
            SetTitle(directoryNode.DirectoryName);
        }

        private void Accept_Button_Click(object sender, RoutedEventArgs e)
        {
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
