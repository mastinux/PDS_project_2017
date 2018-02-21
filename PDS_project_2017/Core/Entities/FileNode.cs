using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core.Entities
{
    public class FileNode
    {
        private string _name;
        private long _dimension;

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
    }
}
