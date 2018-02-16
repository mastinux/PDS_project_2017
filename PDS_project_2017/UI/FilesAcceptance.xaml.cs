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
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using PDS_project_2017.Core;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per FilesAcceptance.xaml
    /// </summary>
    public partial class FilesAcceptance : Window
    {
        private string _fileName;
        private string _destinationDir;
        
        public string DestinationDir { get => _destinationDir;}

        public FilesAcceptance(string fileName)
        {
            InitializeComponent();

            _fileName = fileName;
            _destinationDir = null;

            Title = String.Format("Accept \"{0}\"", fileName);
        }

        private void Accept_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Refuse_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DestinationDir_Button_Click(object sender, RoutedEventArgs e)
        {
            //_destinationDir = UserUtils.RetrieveDirectoryLocation();

            Application.Current.Dispatcher.Invoke(() => { _destinationDir = UserUtils.RetrieveDirectoryLocation(); });

            Console.WriteLine(_destinationDir);
            
            /*
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            CommonFileDialogResult result;

            Application.Current.Dispatcher.Invoke(() =>
            {
                result = dialog.ShowDialog();

                if (result.ToString() == "Ok")
                {
                    _destinationDir = dialog.FileName;
                }
            });
            */
        }
    }
}
