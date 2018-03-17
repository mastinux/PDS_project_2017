using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core.Entities
{
    public class FileNode : IEquatable<FileNode>
    {
        private string _senderUserName;
        private string _receiverUserName;
        private string _name;
        private long _dimension;
        private string _mimeType;

        public string SenderUserName
        {
            get => _senderUserName;
            set => _senderUserName = value;
        }

        public string ReceiverUserName
        {
            get => _receiverUserName;
            set => _receiverUserName = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public long Dimension
        {
            get => _dimension;
            set => _dimension = value;
        }

        public string MimeType
        {
            get => _mimeType;
            set => _mimeType = value;
        }

        public bool Equals(FileNode other)
        {
            if (other == null)
                return false;

            if (SenderUserName == other.SenderUserName && ReceiverUserName == other.ReceiverUserName && Name == other.Name && Dimension == other.Dimension && MimeType == other.MimeType)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() & Dimension.GetHashCode() & SenderUserName.GetHashCode();
        }
    }
}
