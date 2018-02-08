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
        // TODO fix share button position under all users images

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
            udpRequester.addUserEvent += AddAvailableAddUser;
            udpRequester.cleanUsersEvent += cleanAvailableUsers;

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

        public void AddAvailableAddUser(User newAvailableUser)
        {
            Console.WriteLine(this.GetType().Name + ": updating observable collection with user " + newAvailableUser.Name);

            // checking if user already exists in the observable collection
            foreach (var user in AvailableUsers)
            {
                // TODO test purpose - use Id instead of Name
                //if (user.Id.Equals(newAvailableUser.Id))
                if (user.Name.Equals(newAvailableUser.Name))
                {
                    user.Name = newAvailableUser.Name;
                    user.Image = newAvailableUser.Image;

                    printAvailableUsers();

                    return;
                }
            }

            // adding new available user
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                AvailableUsers.Add(newAvailableUser);
            }));
            
            printAvailableUsers();
        }

        public void cleanAvailableUsers()
        {
            Console.WriteLine("Cleaning available users");

            DateTime startTime = DateTime.Now;
            
            for (var i = availableUsers.Count - 1; i >= 0; i--)
            {
                var u = availableUsers[i];

                if (startTime.Subtract(u.LastUpTime).Seconds >= UdpRequester.UPDATE_INTERVAL_SECONDS + 1)
                {
                    // removing expired user
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        availableUsers.Remove(u);
                    }));
                }
                    
            }

            Console.WriteLine("Available users cleaned");
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
