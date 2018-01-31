using Microsoft.Shell;
using SimpleContextMenu;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDS_project_2017
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        // file type to register
        const string FileType = "*";

        // context menu name in the registry
        const string KeyName = "Lan_Sharing_Application";

        // context menu text
        const string MenuText = "Share With...";

        private const string Unique = "Lan_Sharing_Application";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();

                // Register Shell Context Menu

                // full path to self, %L is placeholder for selected file
                string menuCommand = string.Format(
                    "\"{0}\" \"%L\"", System.Reflection.Assembly.GetExecutingAssembly().Location);

                // register the context menu
                FileShellExtension.Register(App.FileType,
                    App.KeyName, App.MenuText,
                    menuCommand);

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            ////  Builder for the output.
            //var builder = new StringBuilder();

            //foreach (var arg in args)
            //{
            //    //  Count the lines.
            //    builder.AppendLine(string.Format("File to send: {0}", arg));
            //}

            //  Show the ouput.
            //MessageBox.Show(builder.ToString());

            MessageBox.Show(string.Format("File to send: {0}", args[1]));

            return true;
        }

        #endregion
    }
}
