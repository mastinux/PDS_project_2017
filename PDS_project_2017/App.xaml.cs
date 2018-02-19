﻿using Microsoft.Shell;
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
        private MainWindow mainWindow;
        private UsersSelection usersSelection;
        // file type to register
        const string FileType = "*";

        // context menu name in the registry
        const string KeyName = "Lan_Sharing_Application";

        // context menu text
        const string MenuText = "Send using Lan Sharing Application";

        private const string Unique = "Lan_Sharing_Application";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                // Register Shell Context Menu

                // full path to self, %L is placeholder for selected file
                string menuCommand = string.Format(
                    "\"{0}\" \"%L\"", System.Reflection.Assembly.GetExecutingAssembly().Location);

                // register the context menu
                FileShellExtension.Register(App.FileType,
                    App.KeyName, App.MenuText,
                    menuCommand);

                // register the context menu
                FileShellExtension.Register("Directory",
                    App.KeyName, App.MenuText,
                    menuCommand);

                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new MainWindow();
            if(e.Args.Length != 0)
                ProcessCommand(e.Args[0]);
        }

        #region ISingleInstanceApp Members

        private void ProcessCommand(string arg)
        {

            //if (args.Length == 0 || string.Compare(args[0], "-register", true) == 0)
            //{

            //}
            //MessageBox.Show(string.Format("File to send: {0}", arg));

            usersSelection = new UsersSelection(arg);
            // visualize window
            usersSelection.Show();
            // put window in foreground
            usersSelection.Activate();
        }

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

            //check if there are args
            if (args.Count == 2)
                ProcessCommand(args[1]);

            return true;
        }

        #endregion
    }
}
