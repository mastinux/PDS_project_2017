using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDS_project_2017.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;

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

            DataContext = this;

            _path = path;

            // parse path and set in window title
            SetPathInTitle();

            // user collection
            AvailableUsers = new ObservableCollection<User>();

            // _availableUserView maintains _availableUsers ordered in UI
            _availableUsersView = CollectionViewSource.GetDefaultView(_availableUsers);
            _availableUsersView.SortDescriptions.Add(new SortDescription("LastUpTime", ListSortDirection.Ascending));
            
            // udp socket requesting available users
            _udpRequester = new UdpRequester();
            _udpRequester.AddUserEvent += AddAvailableAddUser;
            _udpRequester.CleanUsersEvent += CleanAvailableUsers;

            // background thread
            Thread udpListenerThread = new Thread(_udpRequester.RetrieveAvailableUsers);
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();

            Top = Constants.SENDER_WINDOW_TOP;
            Left = Constants.SENDER_WINDOW_LEFT;
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

                if (startTime.Subtract(u.LastUpTime).Seconds >= Constants.AVAILABLE_USERS_UPDATE_INTERVAL + 3)
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

                int i = 0;

                foreach( User u  in selected)
                {
                    if (_isDirectory)
                    {
                        // directory
                        TCPSender tcpSender = new TCPSender(u.Id, Constants.TRANSFER_TCP_PORT, u.Name, _path);

                        Thread tcpSenderThread = new Thread(tcpSender.SendDirectory);
                        tcpSenderThread.SetApartmentState(ApartmentState.STA);
                        tcpSenderThread.IsBackground = true;
                        tcpSenderThread.Start();
                    }
                    else
                    {
                        // single file
                        TCPSender tcpSender = new TCPSender(u.Id, Constants.TRANSFER_TCP_PORT, u.Name, _path);
                        tcpSender.SetIndex(i + 1);

                        Thread tcpSenderThread = new Thread(tcpSender.SendFile);
                        tcpSenderThread.SetApartmentState(ApartmentState.STA);
                        tcpSenderThread.IsBackground = true;
                        tcpSenderThread.Start();

                        /*
                        TCPSender testTcpSender = new TCPSender(u.Id, Constants.TRANSFER_TCP_TEST_PORT, u.Name, _path);
                        tcpSender.SetIndex(i * 2 + 1);

                        Thread testTcpSenderThread = new Thread(testTcpSender.SendFile);
                        testTcpSenderThread.SetApartmentState(ApartmentState.STA);
                        testTcpSenderThread.IsBackground = true;
                        testTcpSenderThread.Start();
                        */
                    }

                    i++;
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

        private void SelectAll_Button_Click(object sender, RoutedEventArgs e)
        {
            listNeighborSelection.SelectAll();
        }

        private void DeselectAll_Button_Click(object sender, RoutedEventArgs e)
        {
            listNeighborSelection.UnselectAll();
        }
    }
}
