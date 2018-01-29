using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    public class User
    {
        private String _id;
        private String _name;
        private String _image;

        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Image { get => _image; set => _image = value; }
    }
}
