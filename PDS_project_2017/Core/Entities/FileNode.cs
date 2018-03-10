using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core.Entities
{
    public class FileNode
    {
        private string _senderUserName;
        private string _name;
        private long _dimension;
        private string _mimeType;

        public string SenderUserName
        {
            get => _senderUserName;
            set => _senderUserName = value;
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
    }
}
