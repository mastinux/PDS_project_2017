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

    /*
     * this class describe a file transfer for both sender and receiver
     */
    public class FileTransfer : INotifyPropertyChanged
    {
        private FileNode _fileNode;
        // value for progress bar in main window
        private double _progress;
        private TimeSpan _remainingTime;
        private string _humanReadableRemainingTime;
        private TransferStatus _status;

        // used to sort transfers in main window
        public DateTime ManagementDateTime { get; set; }

        public bool Sending { get; set; }

        public string DestinationDirectoryPath { get; set; }

        // updated if user cancel transfer
        public bool ContinueFileTransfer { get; set; }
        
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

        public TransferStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

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
                if (propertyName.Equals("Status") && Status != TransferStatus.Removed)
                    StatusChangedEvent(this);
        }
    }
}
