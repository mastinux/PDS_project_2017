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
using MahApps.Metro.Controls;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per TransferProgress.xaml
    /// </summary>
    public partial class TransferProgress : MetroWindow
    {
        public TransferProgress(string userName, string fileName)
        {
            // TODO read some material about uwp
            // to be used in combination with wpf

            InitializeComponent();

            DataContext = this;

            DestinationUser.Text = userName;
            FileName.Text = fileName;
            ProgressBar.Value = 0;
        }

        // TODO update progress bar value
    }
}
