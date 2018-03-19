using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PDS_project_2017.Core;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.UI.Utils
{
    public class WindowUtils
    {
        /*
        public static TransferProgress InitTransferProgressWindow(FileNode fileNode, int index, object o)
        {
            TransferProgress transferProgressWindow = null;
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                transferProgressWindow = new TransferProgress(fileNode, o);
                transferProgressWindow.SetIndex(index);
                transferProgressWindow.Show();
            }));

            return transferProgressWindow;
        }

        public static void CloseTransferProgressWindow(TransferProgress transferProgressWindow)
        {
            Thread.Sleep(Constants.TRANSFER_TCP_COMPLETED_TRANSFER_DELAY);

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                transferProgressWindow.Close();
            }));
        }
        */

        public static void ShowMainWindow(bool sending = true)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow mainWindow = App.Current.MainWindow as MainWindow;

                mainWindow.ShowWindow(sending);
            }));
        }
    }
}
