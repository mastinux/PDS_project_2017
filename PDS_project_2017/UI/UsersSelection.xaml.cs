using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDS_project_2017.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
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
using TreeView = System.Windows.Forms.TreeView;

namespace PDS_project_2017
{
    public partial class UsersSelection : MetroWindow
    {
        private ObservableCollection<User> _availableUsers;
        private ICollectionView _availableUsersView;
        private UdpRequester _udpRequester;
        private string _path;
        private string _pathName;
        private bool _isDirectory;
        
        public ObservableCollection<User> AvailableUsers { get => _availableUsers; set => _availableUsers = value; }

        public UsersSelection(string path)
        {
            InitializeComponent();
            Closing += ClosingUserSelection;

            _path = path;

            // parse path and set in window title
            SetPathInTitle();

            // user collection
            AvailableUsers = new ObservableCollection<User>();

            // _availableUserView maintains _availableUsers ordered in UI
            _availableUsersView = CollectionViewSource.GetDefaultView(_availableUsers);
            _availableUsersView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            DataContext = this;
            
            // udp socket requesting available users
            _udpRequester = new UdpRequester();
            _udpRequester.AddUserEvent += AddAvailableAddUser;
            _udpRequester.CleanUsersEvent += CleanAvailableUsers;

            // background thread
            Thread udpListenerThread = new Thread(_udpRequester.RetrieveAvailableUsers);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();
        }

        private void SetPathInTitle()
        {
            FileAttributes attr = File.GetAttributes(_path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                _isDirectory = true;
                _pathName = new DirectoryInfo(_path).Name;
            }
            else
            {
                _isDirectory = false;
                _pathName = System.IO.Path.GetFileName(_path);
            }

            Title = String.Format("Share \"{0}\" with:", _pathName);
        }

        public void AddAvailableAddUser(User newAvailableUser)
        {
            // checking if user already exists
            foreach (var user in AvailableUsers)
            {
                // TODO test purpose - on production environment use Id instead of Name
                //if (user.Id.Equals(newAvailableUser.Id))
                if (user.Name.Equals(newAvailableUser.Name))
                {
                    user.Name = newAvailableUser.Name;
                    user.Image = newAvailableUser.Image;
                    user.LastUpTime = DateTime.Now;

                    return;
                }
            }

            // adding new available user
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                AvailableUsers.Add(newAvailableUser);
            }));
        }

        public void CleanAvailableUsers()
        {
            DateTime startTime = DateTime.Now;
            
            for (var i = _availableUsers.Count - 1; i >= 0; i--)
            {
                var u = _availableUsers[i];

                if (startTime.Subtract(u.LastUpTime).Seconds >= Constants.AVAILABLE_USERS_UPDATE_INTERVAL + 1)
                {
                    // removing expired user
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        _availableUsers.Remove(u);
                    }));
                }
            }
        }
        
        private void Share_Button_Click(object sender, RoutedEventArgs e)
        {
            // terminating thread loop
            _udpRequester.StopRequesting();

            List<User> selected = null;
            //List<SendingFile> sendingFiles = null;

            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                selected = listNeighborSelection.SelectedItems.Cast<User>().ToList();

                foreach( User u  in selected)
                {
                    if (_isDirectory)
                    {
                        // directory
                        TCPSender tcpSender = new TCPSender(u.Id, _path);

                        Thread tcpSenderThread = new Thread(tcpSender.SendDirectory);
                        tcpSenderThread.IsBackground = true;
                        tcpSenderThread.Start();
                    }
                    else
                    {
                        // single file
                        TCPSender tcpSender = new TCPSender(u.Id, _path);

                        Thread tcpSenderThread = new Thread(tcpSender.SendFile);
                        tcpSenderThread.IsBackground = true;
                        tcpSenderThread.Start();
                    }
                }

                Close();
            }
            else
                this.ShowMessageAsync("Ops", "Choose at least one user");
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            // terminating thread loop
            _udpRequester.StopRequesting();

            this.Close();
        }

        private void ClosingUserSelection(object sender, CancelEventArgs e)
        {
            // terminating thread loop
            _udpRequester.StopRequesting();
        }
    }
}
