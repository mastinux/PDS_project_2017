using PDS_project_2017.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PDS_project_2017
{
    /// <summary>
    /// Logica di interazione per UsersSelection.xaml
    /// </summary>
    public partial class UsersSelection : Window
    {
        private ObservableCollection<User> availableUsers;
        private UdpRequester udpRequester;
        public delegate void AddNewUserCallback(User newAvailableUser);

        public UsersSelection()
        {
            InitializeComponent();

            availableUsers = new ObservableCollection<User>();

            udpRequester = new UdpRequester(new AddNewUserCallback(AddAvailableUserCallback));
            
            udpRequester.retrievaAvailableUsers();
        }

        public void AddAvailableUserCallback(User newAvailableUser)
        {
            Console.WriteLine(this.GetType().Name + ": updating observable collection with user " + newAvailableUser.Name);

            // checking if user already exists in the observable collection
            foreach (var user in availableUsers)
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
            availableUsers.Add(newAvailableUser);

            printAvailableUsers();
        }
        
        private void printAvailableUsers()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Current available users:");

            foreach (var user in availableUsers)
            {
                Console.WriteLine(" - " + user.Name + " on " + user.Id + " with " + user.Image);
            }

            Console.WriteLine("=================================================================");
        }
    }
}
