using MahApps.Metro.Controls;
using PDS_project_2017.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<User> AvailableUsers { get => availableUsers; set => availableUsers = value; }

        public UsersSelection()
        {
            InitializeComponent();

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

        public void AddAvailableUser(User newAvailableUser)
        {
            Console.WriteLine(this.GetType().Name + ": updating observable collection with user " + newAvailableUser.Name);

            // checking if user already exists in the observable collection
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
    }
}
