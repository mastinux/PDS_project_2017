using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PDS_project_2017.Core.Entities
{
    public enum TransferStatus
    {
        Pending,
        Completed,
        Cancelled
    };

    public class FileTransfer : INotifyPropertyChanged
    {
        private FileNode _fileNode;
        private double _progress;
        private TimeSpan _remainingTime;
        private string _humanReadableRemainingTime;
        private bool _continueFileTransfer;
        private TransferStatus status;
        

        public FileNode File
        {
            get => _fileNode;
            set
            {
                _fileNode = value;
                NotifyPropertyChanged();
            }
        }
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
            }
        }

        public bool ContinueFileTransfer
        {
            get => _continueFileTransfer;
            set => _continueFileTransfer = value;
        }

        public TransferStatus Status
        {
            get => status;
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
