using System;

namespace PDS_project_2017.Core.Entities
{
    public class FileNode : IEquatable<FileNode>
    {
        public string SenderUserName { get; set; }

        public string ReceiverUserName { get; set; }

        public string Name { get; set; }

        public long Dimension { get; set; }

        public string MimeType { get; set; }

        public bool Equals(FileNode other)
        {
            if (other == null)
                return false;

            if (SenderUserName == other.SenderUserName && 
                ReceiverUserName == other.ReceiverUserName && 
                Name == other.Name && 
                Dimension == other.Dimension && 
                MimeType == other.MimeType)
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
