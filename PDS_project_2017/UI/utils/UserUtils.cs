using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PDS_project_2017.Core
{
    class UserUtils
    {
        public static String RetrieveDirectoryLocation()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result.ToString() == "Ok")
            {
                return dialog.FileName;
            }
            else
                return null;
        }
    }
}
