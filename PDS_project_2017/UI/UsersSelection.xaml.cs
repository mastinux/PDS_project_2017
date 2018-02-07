using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDS_project_2017.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per UsersSelection.xaml
    /// </summary>
    public partial class UsersSelection : MetroWindow
    {
        private ObservableCollection<User> availableUsers;
        private UdpRequester udpRequester;
        private string pathName;
        private bool isDirectory;
        public ObservableCollection<User> AvailableUsers { get => availableUsers; set => availableUsers = value; }

        public UsersSelection(string path)
        {
            InitializeComponent();
            ProcessFilePath(path);
            AvailableUsers = new ObservableCollection<User>();
            DataContext = this;
            
            // udp socket requesting available users
            udpRequester = new UdpRequester();
            udpRequester.userEvent += AddAvailableUser;

            // launching background thread
            Thread udpListenerThread = new Thread(udpRequester.retrieveAvailableUsers);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();
        }

        private void ProcessFilePath(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                isDirectory = true;
                pathName = new DirectoryInfo(path).Name;
            }
            else
            {
                isDirectory = false;
                pathName = System.IO.Path.GetFileName(path);
            }
            Title = String.Format("Share \"{0}\" with:", pathName);
        }

        public void AddAvailableUser(User newAvailableUser)
        {
            Console.WriteLine(this.GetType().Name + ": updating observable collection with user " + newAvailableUser.Name);

            // checking if user already exists in the observable collection
            // TODO remove this comment
            /*
            foreach (var user in AvailableUsers)
            {
                if (user.Id.Equals(newAvailableUser.Id))
                {
                    user.Name = newAvailableUser.Name;
                    user.Image = newAvailableUser.Image;

                    printAvailableUsers();

                    return;
                }
            }
            */

            // adding new available user
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                AvailableUsers.Add(newAvailableUser);
            }));
            
            printAvailableUsers();
        }
        
        private void printAvailableUsers()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Current available users:");

            foreach (var user in AvailableUsers)
            {
                Console.WriteLine(" - " + user.Name + " on " + user.Id + " with " + user.Image);
            }

            Console.WriteLine("=================================================================");
        }

        private void Select_User_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Selected " + sender);

            // TODO trigger this method from UI
        }

        private void Share_Button_Click(object sender, RoutedEventArgs e)
        {
            List<User> selected = null;
            //List<SendingFile> sendingFiles = null;
            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                selected = listNeighborSelection.SelectedItems.Cast<User>().ToList();
                foreach( User u  in selected)
                {
                    Console.WriteLine("Sending to" + u.Name);
                }
                Hide();
            }
            else
                this.ShowMessageAsync("Ops", "Choose at least one user");
        }
    }
}
