using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PDS_project_2017.Core
{
    public class User
    {
        private String _id;
        private String _name;
        private Image _image;

        public string Id { get => _id; set => _id = value; }

        public string Name { get => _name; set => _name = value; }

        [JsonConverter(typeof(ImageConverter))]
        public Image Image { get => _image; set => _image = value; }
    }
}
