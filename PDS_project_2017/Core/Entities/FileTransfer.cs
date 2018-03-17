using System;
using System.ComponentModel;

namespace PDS_project_2017.Core.Entities
{
    public enum TransferStatus
    {
        Pending,
        Completed,
        Canceled,
        Error,
        Removed
    };

    public class FileTransfer : INotifyPropertyChanged
    {
        private FileNode _fileNode;
        private double _progress;
        private TimeSpan _remainingTime;
        private string _humanReadableRemainingTime;
        private bool _continueFileTransfer;
        private TransferStatus _status;
        private bool _sending;
        private String _savingPath;
        private DateTime _managementDateTime;
        
        public FileNode File
        {
            get => _fileNode;
            set
            {
                _fileNode = value;
                NotifyPropertyChanged("File");
            }
        }
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value;
                HumanReadableRemainingTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                    _remainingTime.Hours,
                    _remainingTime.Minutes,
                    _remainingTime.Seconds);
            }
        }

        public string HumanReadableRemainingTime
        {
            get => _humanReadableRemainingTime;
            set
            {
                _humanReadableRemainingTime = value;
                NotifyPropertyChanged("HumanReadableRemainingTime");
            }
        }

        public bool ContinueFileTransfer
        {
            get => _continueFileTransfer;
            set => _continueFileTransfer = value;
        }

        public TransferStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public DateTime ManagementDateTime { get => _managementDateTime; set => _managementDateTime = value; }
        public bool Sending { get => _sending; set => _sending = value; }
        public string SavingPath { get => _savingPath; set => _savingPath = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void StatusChangedDelegate(FileTransfer fileTransfer);
        public event StatusChangedDelegate StatusChangedEvent;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            if (StatusChangedEvent != null)
                if (propertyName.Equals("Status"))
                    StatusChangedEvent(this);
        }
    }
}
