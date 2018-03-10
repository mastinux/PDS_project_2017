using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core.Entities
{
    public class FileTransfer : INotifyPropertyChanged
    {
        private FileNode file;
        private double progress;
        private TimeSpan remainingTime;

        public FileNode File
        {
            get => file;
            set
            {
                file = value;
                NotifyPropertyChanged();
            }
        }
        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                NotifyPropertyChanged();
            }
        }
  
        public TimeSpan RemainingTime { get => remainingTime; set => remainingTime = value; }

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
