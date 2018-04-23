using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDS_project_2017.Core;
using PDS_project_2017.UI.Utils;

namespace PDS_project_2017.UI
{
    public partial class UsersSelection : MetroWindow
    {
        private ObservableCollection<User> _availableUsers;
        private UdpRequester _udpRequester;
        private string _path;
        private string _pathName;
        private bool _isDirectory;
        
        public ObservableCollection<User> AvailableUsers { get => _availableUsers; set => _availableUsers = value; }

        public UsersSelection(string path)
        {
            InitializeComponent();

            Closing += OnClosing;

            DataContext = this;

            _path = path;

            // set directory or file name in title
            SetPathInTitle();

            // user collection
            AvailableUsers = new ObservableCollection<User>();

            // _availableUserView maintains _availableUsers ordered in UI
            ICollectionView availableUsersView = CollectionViewSource.GetDefaultView(_availableUsers);
            availableUsersView.SortDescriptions.Add(new SortDescription("LastUpTime", ListSortDirection.Ascending));
            
            // udp socket requesting available users
            _udpRequester = new UdpRequester();
            _udpRequester.AddUserEvent += ManageAddUserEvent;
            _udpRequester.CleanUsersEvent += CleanExpiredUsers;

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
                _pathName = Path.GetFileName(_path);
            }

            Title = String.Format("Share \"{0}\" with:", _pathName);
        }

        public void ManageAddUserEvent(User newAvailableUser)
        {
            // checking if user already exists
            foreach (var user in AvailableUsers)
            {
                if (user.IPAddress.Equals(newAvailableUser.IPAddress))
                //if (user.Name.Equals(newAvailableUser.Name))
                {
                    // updating already present user
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

        public void CleanExpiredUsers()
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

            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                List<User> selected = listNeighborSelection.SelectedItems.Cast<User>().ToList();

                foreach( User u  in selected)
                {
                    TCPSender tcpSender = new TCPSender(u.IPAddress, Constants.TRANSFER_TCP_PORT, u.Name, _path);

                    Thread tcpSenderThread;

                    if (_isDirectory)
                        tcpSenderThread = new Thread(tcpSender.SendDirectory);
                    else
                        tcpSenderThread = new Thread(tcpSender.SendFile);

                    tcpSenderThread.SetApartmentState(ApartmentState.STA);
                    tcpSenderThread.IsBackground = true;
                    tcpSenderThread.Start();
                }

                Close();

                InterfaceUtils.ShowMainWindow();
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

        private void OnClosing(object sender, CancelEventArgs e)
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
